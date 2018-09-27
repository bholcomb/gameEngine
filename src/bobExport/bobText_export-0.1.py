#! BPY

#add-on information
bl_info = {
    "name": "BOB Exporter (text)",
    "description": "Export mesh data with UV's into Binary Object Format (Bob) text format",
    "author": "Bob Holcomb",
    "version": (2018, 3, 11),
    "blender": (2, 79, 0),
    "location": "File > Import-Export",
    "warning": "",
    "wiki_url": "",
    "tracker_url": "",
    "category": "Import-Export"}

import bpy
import math
import mathutils

import bpy_extras.io_utils
from bpy_extras.io_utils import ExportHelper,orientation_helper_factory, axis_conversion
from bpy.props import StringProperty, BoolProperty, EnumProperty
from bpy.types import Operator

BOBOrientationHelper = orientation_helper_factory(
    "BOBOrientationHelper", axis_forward='-Z', axis_up='Y'
)

###########################################################
#helper functions
###########################################################
def indent(num, str):
   padding = num * '   '
   return padding + str + '\n'
   
def r3d(v):
    return round(v[0],6), round(v[1],6), round(v[2],6)

def r2d(v):
    return round(v[0],6), round(v[1],6)
    
def grouper(seq, size):
    return (seq[pos:pos + size] for pos in range(0, len(seq), size))

def getBonesInOrder(skel):
   if(skel == None):
      return {}
      
   ret = []
   idx = 0
   rootBones = []
   for b in skel.bones:
      if(b.parent == None):
         rootBones.append(b)
         ret.append({'index': idx, 'bone': b})
         idx += 1
         #print("Adding root bone {}".format(b.name))
         
   for b in rootBones:
      #print("Root bone {}".format(b.name))
      childBones = b.children_recursive
      for cb in childBones:
         ret.append({'index': idx, 'bone': cb})
         idx += 1
         #print("   Adding child bone {}".format(cb.name))
         
   #for b in ret:
      #print("index: {} bone: {}".format(b['index'], b['bone'].name))
      
   return ret;
      
def findBoneIndex(boneName, bonesInOrder):
   if(boneName == ""):
      return -1
      
   for b in bonesInOrder:
      if(b['bone'].name == boneName):
         return b['index']

   print("Failed to find bone {} in bone ordered list".format(boneName))
   raise

def matrixToString(mat):
   ret = ""
   for i in range(0, 4):
      for j in range(0, 4):
         ret += str(mat[j][i])
         if(i != 4 and j != 4):
            ret += ", "

   return ret


class Settings:
    __slots__ = (
        "global_matrix",
        "filepath",
        "context",
        "exportAnimations",
        "armObj",
        "skeleton"
    )
    
    def __init__(self):
        self.filepath = ""
        self.context = bpy.context
        self.global_matrix = mathutils.Matrix.Identity(4)
        self.exportAnimations = False
        self.armObj = None
        self.skeleton = None

###########################################################
#BOB known flags for chunks
###########################################################

class ModelFlags:
   ANIMATED = 0x01
   
class ActionFlags:
   LOOP = 0x01


###########################################################
#BOB chunk definitions
###########################################################
class Vertex:
   __slots__ = (
      "layout",
      "pos",
      "nor",
      "uv",
      "boneIndexes",
      "boneWeights"
   )
   
   def __init__(self, pos, nor, uv, bidx = None, bweights = None):
      if(bidx == None): 
         self.layout = "V3N3T2"
      else: 
         self.layout = "V3N3T2B4W4"
      self.pos = pos
      self.nor = nor
      self.uv = uv
      self.boneIndexes = bidx
      self.boneWeights = bweights

   def __repr__(self):
      if(self.boneIndexes != None):
         return "{:6f}, {:6f}, {:6f},      {:6f}, {:6f}, {:6f},      {:6f}, {:6f},      {}, {}, {}, {},      {:6f}, {:6f}, {:6f}, {:6f},".format(
         self.pos[0], self.pos[1], self.pos[2], 
         self.nor[0], self.nor[1], self.nor[2], 
         self.uv[0], self.uv[1],
         self.boneIndexes[0], self.boneIndexes[1], self.boneIndexes[2], self.boneIndexes[3],
         self.boneWeights[0], self.boneWeights[1], self.boneWeights[2], self.boneWeights[3])
      else:
         return "{:6f}, {:6f}, {:6f},      {:6f}, {:6f}, {:6f},      {:6f}, {:6f},".format(
         self.pos[0], self.pos[1], self.pos[2], 
         self.nor[0], self.nor[1], self.nor[2], 
         self.uv[0], self.uv[1])
         
   def __eq__(self, other):
      if(self.pos != other.pos): return False
      if(self.nor != other.nor): return False
      if(self.uv != other.uv): return False
      if(self.boneIndexes != other.boneIndexes): return false;
      if(self.boneWeights != other.boneWeights): return false;
      
      return True
      
   def __ne__(self, other):
      return not self.__eq__(other)

class BOBChunk:
   __slots__= (
      "type",
      "name",
      "version",
      "flags"
   )

   def __init__(self):
      self.type=""
      self.name =""
      self.version = 1
      self.flags = 0

   def write(self, file):
      file.write(indent(3, "type = \"{}\";".format(self.type)))
      file.write(indent(3, "name = \"{}\";".format(self.name)))
      file.write(indent(3, "version = {};".format(self.version)))
      file.write(indent(3, "flags = {};".format(self.flags)))

class BOBMaterial:
   __slots__ = (
      "name",
      "ambient",
      "diffuse",
      "spec", 
      "emission",
      "shininess",
      "alpha",
      "diffuseTexture",
      "specularTexture",
      "normalTexture",
      "emissionTexture",
      "alphaTexture"
   )
   
   def __init__(self):
      self.name = ""
      self.ambient = []
      self.diffuse = []
      self.spec = []
      self.emission = []
      self.shininess = 0
      self.alpha = 0
      self.diffuseTexture = ""
      self.specularTexture = ""
      self.normalTexture = ""
      self.emissionTexture = ""
      self.alphaTexture = ""
   
   def parse(self, m):
      self.name = m.name
      self.ambient = [m.ambient, m.ambient, m.ambient]
      self.diffuse= m.diffuse_intensity * m.diffuse_color
      self.spec = m.specular_intensity * m.specular_color
      self.emission = m.emit * m.diffuse_color
      self.shininess = m.specular_hardness
      self.alpha = m.alpha
      
      for tex in m.texture_slots:
         if tex and tex.use and tex.texture.type  == "IMAGE":
            texName = tex.texture.image.filepath.strip('/')
            if(tex.use_map_color_diffuse or tex.use_map_diffuse):
               self.diffuseTexture = texName
            elif(tex.use_map_color_spec or tex.use_map_specular):
               self.specularTexture = texName
            elif(tex.use_map_normal):
               self.normalTexture = texName
            elif(tex.use_map_emit or tex.use_map_emission):
               self.emissionTexture = texName
            elif(tex.use_map_translucency or tex.use_map_alpha):
               self.alphaTexture = texName

   def write(self, file):
      file.write(indent(4, "{"))
      file.write(indent(5, "name = \"{}\";".format(self.name)))
      file.write(indent(5, "ambient = {{ r={:.4f}, g={:.4f}, b={:.4f} }};".format(*(self.ambient))))
      file.write(indent(5, "diffuse = {{ r={:.4f}, g={:.4f}, b={:.4f} }};".format(*(self.diffuse))))
      file.write(indent(5, "spec = {{ r={:.4f}, g={:.4f}, b={:.4f} }};".format(*(self.spec))))
      file.write(indent(5, "emission = {{ r={:.4f}, g={:.4f}, b={:.4f} }};".format(*(self.emission))))
      file.write(indent(5, "shininess = {:.4f};".format(self.shininess)))
      file.write(indent(5, "alpha = {:.4f};".format(self.alpha)))
      if(self.diffuseTexture != ""):
         file.write(indent(5, "diffuseTexture = \"{}\";".format(self.diffuseTexture)))
      if(self.specularTexture != ""):
         file.write(indent(5, "specularTexture = \"{}\";".format(self.specularTexture)))
      if(self.normalTexture != ""):
         file.write(indent(5, "normalTexture = \"{}\";".format(self.normalTexture)))
      if(self.emissionTexture != ""):
         file.write(indent(5, "emissionTexture = \"{}\";".format(self.emissionTexture)))
      if(self.alphaTexture != ""):
         file.write(indent(5, "alphaTexture = \"{}\";".format(self.alphaTexture)))
      file.write(indent(4, "};"))

class BOBMesh:
   __slots__ = (
      "material",
      "indexStart",
      "indexCount", 
   )
   
   def __init__(self):
      self.material = ""
      self.indexStart = 0
      self.indexCount = 0
      
   def write(self, file):
      file.write(indent(4, "{"))
      file.write(indent(5, "material = \"{}\";".format(self.material)))
      file.write(indent(5, "indexStart = {};".format(self.indexStart)))
      file.write(indent(5, "indexCount = {};".format(self.indexCount)))
      file.write(indent(4, "};"))
  
class BOBModel(BOBChunk):
   __slots__= (
      "primativeType",
      "vertexFormat",
      "indexFormat",
      "vertexCount",
      "indexCount",
      "meshes",
      "materials",
      "verts",
      "indexes",
      "skeleton"
   )
   
   def __init__(self):
      BOBChunk.__init__(self)
      self.type = "model"
      self.primativeType = "TRI"
      self.vertexFormat = "V3N3T2"
      self.indexFormat = "UInt16"
      self.indexCount =0
      self.vertexCount=0
      self.meshes=[]
      self.materials = []
      self.verts = []
      self.indexes = []
      self.skeleton = ""
      
   def addVertex(self, vert):
      index = [n for n,x in enumerate(self.verts) if vert == x]
      #index = None
      if( not index):
         self.verts.append(vert)
         return len(self.verts) - 1
         
      return index[0]

   def parse(self, obj, settings):
      #does the model have an armature parent
      skeleton = obj.find_armature()
      
      #create a temp copy to work on
      # We don't apply modifiers for a skinned mesh because we need the vertex positions
      # before they are deformed by the armature modifier in order to export the proper
      # bind pose. This does mean that modifiers preceding the armature modifier are ignored,
      # but the Blender API does not provide a reasonable way to retrieve the mesh at an
      # arbitrary stage in the modifier stack.
      
      mesh = obj.to_mesh(settings.context.scene ,skeleton == None, "PREVIEW")
         
      #rotate to our selected space
      mesh.transform(settings.global_matrix)

      self.name = obj.name
      matIndexes=[]
      bones = None
      
      #get the bones for this model
      if(skeleton != None):
         self.flags |= ModelFlags.ANIMATED
         self.skeleton = skeleton.name
         bones = getBonesInOrder(skeleton.data)

      #get the mesh data structs
      modUV = mesh.uv_layers.active.data
      modVert = mesh.vertices
      modFaces = mesh.polygons
      modLoops = mesh.loops
      modGroups = obj.vertex_groups
      
      #parse the materials
      for mat in mesh.materials:
         m = BOBMaterial()
         m.parse(mat)
         self.materials.append(m)
         matIndexes.append([]) #add an empty array for faces of this material
         #print("Added material: " + m.name)
      
      #break model into meshes by material
      for face in modFaces:
         #error if it's not a triangle face
         if(face.loop_total != 3):
            print("Face is not a triangle")
            raise
            
         #find index list by material
         indexList = matIndexes[face.material_index]
         
         #build the vertex and add the indexes to the material index list
         for loop_index in range(face.loop_start, face.loop_start + face.loop_total):
            vert_index = modLoops[loop_index].vertex_index
            pos = modVert[vert_index].co
            nor = modVert[vert_index].normal
            uv = modUV[loop_index].uv
            #uv[1] = 1.0 - uv[1] #flip the meaning of the Y axis since blender texture origin is top left and openGL is bottom left
            
            idx = 0
            if(skeleton != None):
               bids = []
               bweights = []
               for vgroup in modVert[vert_index].groups:
                  if(vgroup.weight > 0 and len(bids) < 4):
                     bids.append(findBoneIndex(modGroups[vgroup.group].name, bones))
                     bweights.append(vgroup.weight)
               
               #add zero weight bone ids to fill out data struct
               while(len(bids) < 4):
                  bids.append(-1)
                  bweights.append(0)
                     
               idx = self.addVertex(Vertex(pos, nor, uv, bids, bweights))
            else:
               idx = self.addVertex(Vertex(pos, nor, uv)) #should return index of vert in the vert list even if its already added
            indexList.append(idx)
            
      #create the mesh data structures based on the material indexes
      for idx, val in enumerate(matIndexes):
         mesh = BOBMesh()
         mesh.material = self.materials[idx].name
         mesh.indexStart = len(self.indexes)
         mesh.indexCount = len(val)
         for i in val:
            self.indexes.append(i)
         self.meshes.append(mesh)
      
      #get some numbers about verts/index
      self.vertexCount = len(self.verts)
      self.indexCount = len(self.indexes)
      
      if(self.vertexCount >= 65535 ): #do we need to use 32 bit ints to describe indexes
         self.indexFormat = "UInt32"
      
      #cleanup our copy
      del mesh

   def write(self, file):
      file.write(indent(2, "{"))
      BOBChunk.write(self, file) #call base class
      file.write(indent(3, "primativeType = \"TRI\";"))
      file.write(indent(3, "vertexFormat = \"{}\";".format(self.verts[0].layout))) #all verts should use the same layout
      file.write(indent(3, "indexFormat = \"UInt16\";"))
      file.write(indent(3, "vertexCount = {};".format(self.vertexCount)))
      file.write(indent(3, "indexCount = {};".format(self.indexCount)))
      if(self.verts[0].layout == "V3N3T2B4W4"):
         file.write(indent(3, "skeleton = \"{}\";".format(self.skeleton)))
      
      file.write(indent(3, "meshes={"))
      for m in self.meshes:
         m.write(file)
      file.write(indent(3, "};"))
      
      file.write(indent(3, "materials = {"))
      for m in self.materials:
         m.write(file)
      file.write(indent(3, "};"))
      
      file.write(indent(3, "verts={"))
      for v in self.verts:
         file.write(indent(4, str(v)))
      file.write(indent(3, "};"))
      
      file.write(indent(3, "indexes={"))
      for idxGroup in grouper(self.indexes, 3):
         file.write(indent(4, "{},  {},  {}, ".format(idxGroup[0] ,idxGroup[1], idxGroup[2]))) 
      file.write(indent(3, "};"))
      
      file.write(indent(2, "};"))

class Bone:
   __slots__ = (
      "name",
      "parentIdx",
      "parentName",
      "matrix"
   )

   def __init__(self):
      self.name = ""
      self.parentIdx = -1
      self.parentName = ""
      self.matrix = []
      
   def parse(self, bone):
      self.name = bone.name
      if(bone.parent != None):
         self.parentName = bone.parent.name
      else:
         self.parentName = ""
   
   def write(self, file):
      file.write(indent(4, "{"))
      file.write(indent(5, "name = \"{}\";".format(self.name)))
      file.write(indent(5, "parent = {};".format(self.parentIdx)))
      file.write(indent(5, "matrix = {{ {} }};".format(matrixToString(self.matrix))))
      file.write(indent(4, "};"))
  
class BOBSkeleton(BOBChunk):
   __slots__ = (
      "bones"
   )

   def __init__(self):
      BOBChunk.__init__(self)
      self.type="skeleton"
      self.bones = []

   def parse(self, obj, settings):
      arm = obj.data.copy()
      arm.transform(settings.global_matrix)

      self.name = obj.name
      bonesInOrder = getBonesInOrder(arm)
      for b in bonesInOrder:
         bone = Bone()
         bone.parse(b['bone'])
         bone.parentIdx = findBoneIndex(bone.parentName, bonesInOrder);
         
         #Get matrix of bone relative to parent
         transform = b['bone'].matrix_local.copy()
         parentBone = b['bone'].parent
         if (parentBone):
            transform = parentBone.matrix_local.inverted() * transform
         
         bone.matrix = transform
         self.bones.append(bone)
         
      #cleanup our copy
      del arm

   def write(self, file):
      file.write(indent(2, "{"))
      BOBChunk.write(self, file) #call base class
      file.write(indent(3, "bones = {"))
      for b in self.bones:
         b.write(file)

      file.write(indent(3, "};"))
      file.write(indent(2, "};"))

class Pose():
   __slots__ = (
      "time",
      "pos",
      "ori"
   )

   def __init__(self, time, pos, ori):
      self.time = time
      self.pos = pos.copy()
      self.ori = ori.copy()
   
   def __repr__(self):
      return "{:6f},      {:6f}, {:6f}, {:6f},      {:6f}, {:6f}, {:6f}, {:6f} ".format(
      self.time,
      self.pos[0], self.pos[1], self.pos[2], 
      self.ori[1], self.ori[2], self.ori[3], self.ori[0]) # w component is last
      
class BOBAnimation(BOBChunk):
   __slots__ = (
      "duration",
      "skeleton",
      "events",
      "channels",
      "numBones"
   )
   
   def __init__(self):
      BOBChunk.__init__(self)
      self.type="animation"
      self.duration = 0
      self.skeleton = ""
      self.events = []
      self.channels = {}
      self.numBones = 0   
      
   def parse(self, action, settings):
      self.flags = ActionFlags.LOOP
      self.name = action.name
      scene = settings.context.scene
      self.skeleton = settings.skeleton
      self.numBones = len(settings.skeleton.bones)
      fps = scene.render.fps
       
      arm = settings.armObj
      origArmData = arm.data
      armData = arm.data.copy()
      armData.transform(settings.global_matrix)
      arm.data = armData

      #so we can restore stuff later
      oldStartFrame = scene.frame_start
      oldEndFrame = scene.frame_end
      oldCurrentFrame = scene.frame_current
      oldAction = arm.animation_data.action
      
      #find the bone keyframes
      print("--------------------Finding Keyframes for bones-----------------------------")
      boneKeyframes = {}
      for bone in settings.skeleton.bones:
         bName = bone.name 
         boneKeyframes[bName] = []
         self.channels[bName] = []
         if(bName in action.groups):# are there fcurves?
            g = action.groups[bName]  # get the group of fcurves for this bone
            for c in g.channels: #for each fcurve
               if(c.data_path.endswith("location") or c.data_path.endswith("rotation_quaternion")):  #is it a position/rotation key?
                  for keyframe in c.keyframe_points:
                     if(boneKeyframes[bName].count(keyframe.co[0]) == 0): #is it in the list already?  we want unique keyframes for this bone
                        print("Bone %s has a unique keyframe at frame %d" %(bName, keyframe.co[0]))
                        boneKeyframes[bName].append(int(keyframe.co[0])) # add it to the list
         if(len(boneKeyframes[bName]) == 0): #there were no keyframes for this bone
            boneKeyframes[bName].append(0) #add the 0 frame so we get some channel data
               
      #setup the armature with the current action and then step over the frames getting the pose data
      arm.animation_data.action = action
      startframe, endframe = action.frame_range
      startframe = int(startframe)
      endframe = int(endframe)
      self.duration = (endframe - startframe) / fps
      #print("-------------------------------------------------")
      #print("\n\n")
      #print('Exporting action "%s" frames %d-%d' % (action.name, startframe, endframe))
      #print("\n\n")
      #print("-------------------------------------------------")
      for frame in range(startframe, endframe + 1):
         #print("---------------------Frame: " + str(frame) + "----------------------------")
         scene.frame_set(frame)
         scene.update()
         
         for bone in settings.skeleton.bones:
            bName = bone.name
            
            #does this bone have a keyframe on this frame?
            if(boneKeyframes[bName].count(frame) > 0):
               print("%s has a keyframe on frame %d" % (bName, frame))
               poseBone = arm.pose.bones[bName]

               transform = 0

               boneRef = poseBone.bone.matrix_local.copy()
               boneBasis = poseBone.matrix_basis.copy()
               
               parentBone = poseBone.parent
               if(parentBone == None):
                  transform = boneRef * boneBasis
               else:
                  parentLocal = parentBone.bone.matrix_local.copy()
                  transform = (parentLocal.inverted() * boneRef) * boneBasis

               #print("----------------" + bName + "----------------")
               #print("local matrix: " + str(transform))                    

               parts = transform.decompose()
               pb = Pose(frame/fps, parts[0], parts[1])
               #print("parts: " + str(parts))

               self.channels[bName].append(pb)

      #export markers (or events)
      for m in action.pose_markers:
         self.events.append({'time':m.frame/fps, 'name':m.name})
         
      #cleanup
      scene.frame_set(oldCurrentFrame)
      arm.animation_data.action = oldAction
      arm.data = origArmData
      del armData
      
   def write(self, file):
      file.write(indent(2, "{"))
      BOBChunk.write(self, file) #call base class
      file.write(indent(3, "duration = {};".format(self.duration)))
      file.write(indent(3, "numBones = {};".format(self.numBones)))
      file.write(indent(3, "skeleton = \"{}\";".format(self.skeleton.name)))
      file.write(indent(3, "events = {"))
      for e in self.events:
         file.write(indent(4, "{{ time = {}, name = \"{}\" }};".format(e["time"], e["name"])))
      file.write(indent(3, "};"))
      
      file.write(indent(3, "channels = {"))
      for bone in self.skeleton.bones:
         bName = bone.name 
         file.write(indent(4, "{"))
         for p in self.channels[bName]:
            file.write(indent(5, "{}, ".format(str(p))))
         file.write(indent(4, "};"))
      file.write(indent(3, "};"))
      file.write(indent(2, "};"))
   
class BOBFile:
   __slots__ = (
      "version",
      "registry",
      "chunks"
   )

   def __init__(self):
      self.version = 1
      self.registry = {}
      self.chunks = []
      
   def addChunk(self, chunk):
      self.chunks.append(chunk)
      self.registry[len(self.chunks) -1] = {"name": chunk.name, "type": chunk.type} #use a 0 based array index
      
   def write(self, file):
      file.write(indent(0, "--Binary OBject file export from Blender"))
      file.write(indent(0, "BOB = {"))
      file.write(indent(1, "version = 1;"))
      file.write(indent(1, "registry = {"))
      for k,v in self.registry.items():
         file.write(indent(2, "[{}] = {{name = \"{}\"; type=\"{}\";}};".format(k, v["name"], v["type"]))) 
      file.write(indent(1, "};"))
      file.write(indent(1, "chunks = {"))
      
      for c in self.chunks:
         c.write(file)
      
      file.write(indent(1, "};"))
      file.write(indent(0, "}"))

###########################################################
#Export MESH object. By default export whole scene
###########################################################

def exportBOB(settings):
   scene = settings.context.scene
   bob = BOBFile()
   armObj = None

   for o in scene.objects:
      if(o.type == "MESH"):
         model = BOBModel()
         model.parse(o, settings)
         bob.addChunk(model)

      if(o.type == "ARMATURE"):
         skeleton = BOBSkeleton()
         skeleton.parse(o, settings)
         bob.addChunk(skeleton)
         settings.armObj = o  #save for animations
         settings.skeleton = skeleton #save for animations

   if(settings.exportAnimations == True and settings.armObj != None and settings.skeleton != None):
      for a in bpy.data.actions:
         animation = BOBAnimation()
         animation.parse(a, settings)
         bob.addChunk(animation)

   #open the output file
   file = open(settings.filepath, "w", newline="\n")
   
   #write the file
   bob.write(file)

   #close the file
   file.close()

###########################################################
#export operator for drawing the export menu
###########################################################
class ExportBOB(bpy.types.Operator, ExportHelper, BOBOrientationHelper):
    '''Export a whole scene or single object as a BOB file with normals and texture coordinates'''
    bl_idname = "export.bob"
    bl_label = "Export BOB"

    filename_ext = ".bob"
    filter_glob = StringProperty(default="*.bob", options={'HIDDEN'})
    entire_scene = BoolProperty(name="Entire Scene", description="Export all MESH object (Entire scene)", default=True)
    export_animations = BoolProperty(name="Export Animations", description="Export Animations", default=True)

    @classmethod
    def poll(cls, context):
        return context.active_object != None

    def execute(self, context):
        settings = Settings()
        settings.context = context
        
        filepath = self.filepath
        filepath = bpy.path.ensure_ext(filepath, self.filename_ext)
        settings.filepath = filepath
        settings.exportAnimations = self.export_animations
        
        #rotate everything so that it is -Z forward and Y up (my game engine format)
        settings.global_matrix = bpy_extras.io_utils.axis_conversion(
            to_forward=self.axis_forward,
            to_up=self.axis_up).to_4x4()
        
        exportBOB(settings)
        return {'FINISHED'}

    def draw(self, context):
        layout = self.layout

        row = layout.row()
        row.prop(self, "entire_scene")
        
        col = layout.box().column(align=True)
        col.label('Axis Conversion:', icon='MANIPUL')
        col.prop(self, 'axis_up')
        col.prop(self, 'axis_forward')
        
        row.prop(self, "export_animations")

###########################################################
#add-on registration/unregistration functions
###########################################################
def menu_func_export(self, context):
    self.layout.operator(ExportBOB.bl_idname, text="BOB file (.bob)")


def register():
    bpy.utils.register_module(__name__)

    bpy.types.INFO_MT_file_export.append(menu_func_export)


def unregister():
    bpy.utils.unregister_module(__name__)

    bpy.types.INFO_MT_file_export.remove(menu_func_export)

if __name__ == "__main__":
    register()
