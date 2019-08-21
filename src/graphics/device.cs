using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

using Vulkan;
using static Vulkan.VulkanNative;

using Util;

namespace Graphics
{
    internal unsafe delegate VkResult vkCreateDebugReportCallbackEXT_d(VkInstance instance, VkDebugReportCallbackCreateInfoEXT* createInfo, IntPtr allocatorPtr, out VkDebugReportCallbackEXT ret);
    internal unsafe delegate void vkDestroyDebugReportCallbackEXT_d(VkInstance instance, VkDebugReportCallbackEXT callback, VkAllocationCallbacks* pAllocator);
    internal unsafe delegate VkResult vkDebugMarkerSetObjectNameEXT_d(VkDevice device, VkDebugMarkerObjectNameInfoEXT* pNameInfo);

    public unsafe class Device : IDisposable
    {
        VkInstance myInstance;
        VkPhysicalDevice myPhysicalDevice;
        VkDevice myDevice;
        VkSurfaceKHR mySurface;

        VkPhysicalDeviceProperties myPhysicalDeviceProperties;
        VkPhysicalDeviceFeatures myPhysicalDeviceFeatures;
        VkPhysicalDeviceMemoryProperties myPhysicalDeviceMemProperties;
        uint myGraphicsQueueIndex;
        uint myPresentQueueIndex;
        uint myTransferQueueIndex;
        uint myComputeQueueIndex;
        uint mySparseQueueIndex;
        
        VkDebugReportCallbackEXT myDebugCallbackHandle;
        PFN_vkDebugReportCallbackEXT myDebugCallbackFunc;



        ResourceManager myResourceManager;

        public Device(IntPtr hInstance, IntPtr hwd, bool debug = false)
        {
            createInstance(debug);
            createPhysicalDevice();
            createSurface(hInstance, hwd);
            createLogicalDevice();


            myResourceManager = new ResourceManager(this);
        }

        public void Dispose()
        {

        }

        public ResourceManager resourceManager { get { return myResourceManager; } }

        public VkDevice device { get { return myDevice; } }

        bool createInstance(bool debug)
        {
            Debug.print("Creating instance");

            List<string> extensions = new List<string>(enumerateInstanceExtensions());
            List<string> layers = new List<string>(enumerateInstanceLayers());
            
            VkApplicationInfo applicationInfo = new VkApplicationInfo();
            applicationInfo.apiVersion = new VkVersion(1, 0, 0);
            applicationInfo.applicationVersion = new VkVersion(1, 0, 0);
            applicationInfo.engineVersion = new VkVersion(1, 0, 0);
            applicationInfo.pApplicationName = new FixedUtf8String("Graphics Engine");
            applicationInfo.pEngineName = new FixedUtf8String("Graphics Engine");

            List<string> instanceLayerNames = new List<string>();

            List<string> instanceExtensionNames = new List<string>() {
                    InstanceExtensions.VK_KHR_surface,
                    InstanceExtensions.VK_KHR_win32_surface
                };

            bool debugReporting = false;
            if(debug == true)
            {
                if (layers.Contains(InstanceLayers.VK_LAYER_LUNARG_standard_validation))
                {
                    instanceLayerNames.Add(InstanceLayers.VK_LAYER_LUNARG_standard_validation);
                }

                if (extensions.Contains(InstanceExtensions.VK_EXT_debug_report))
                {
                    instanceExtensionNames.Add(InstanceExtensions.VK_EXT_debug_report);
                    debugReporting = true;
                }
            };

            VkInstanceCreateInfo instanceCI = VkInstanceCreateInfo.New();
            instanceCI.pApplicationInfo = &applicationInfo;
            instanceCI.enabledExtensionCount = (UInt32)instanceLayerNames.Count;
            instanceCI.enabledLayerCount = (UInt32)instanceExtensionNames.Count;
            instanceCI.ppEnabledLayerNames = new FixedUtf8StringArray(instanceLayerNames);
            instanceCI.ppEnabledExtensionNames = new FixedUtf8StringArray(instanceExtensionNames  );

            VkResult result = vkCreateInstance(ref instanceCI, null, out VkInstance testInstance);
            Util.checkResult(result, "Failed to create instance");

            if (debugReporting == true)
            {
                enableDebugCallback();
            }

            return true;
        }

        void enableDebugCallback()
        {
            Info.print("Enabling Vulkan Debug callbacks.");
            myDebugCallbackFunc = DebugCallback;
            IntPtr debugFunctionPtr = Marshal.GetFunctionPointerForDelegate(myDebugCallbackFunc);
            VkDebugReportCallbackCreateInfoEXT debugCallbackCI = VkDebugReportCallbackCreateInfoEXT.New();
            debugCallbackCI.flags = VkDebugReportFlagsEXT.WarningEXT | VkDebugReportFlagsEXT.ErrorEXT;
            debugCallbackCI.pfnCallback = debugFunctionPtr;
            IntPtr createFnPtr;
            using (FixedUtf8String debugExtFnName = "vkCreateDebugReportCallbackEXT")
            {
                createFnPtr = vkGetInstanceProcAddr(myInstance, debugExtFnName);
            }
            if (createFnPtr == IntPtr.Zero)
            {
                return;
            }

            vkCreateDebugReportCallbackEXT_d createDelegate = Marshal.GetDelegateForFunctionPointer<vkCreateDebugReportCallbackEXT_d>(createFnPtr);
            VkResult result = createDelegate(myInstance, &debugCallbackCI, IntPtr.Zero, out myDebugCallbackHandle);
            Util.checkResult(result, "Failed to create debug callback");
        }

        private uint DebugCallback(uint flags, VkDebugReportObjectTypeEXT objectType, ulong @object, UIntPtr location, int messageCode, byte* pLayerPrefix, byte* pMessage, void* pUserData)
        {
            string message = Util.GetString(pMessage);
            VkDebugReportFlagsEXT debugReportFlags = (VkDebugReportFlagsEXT)flags;

            string fullMessage = $"[{debugReportFlags}] ({objectType}) {message}";

            if (debugReportFlags == VkDebugReportFlagsEXT.ErrorEXT)
            {
                throw new Exception("A Vulkan validation error was encountered: " + fullMessage);
            }

            Warn.print(fullMessage);
            return 0;
        }

        void createPhysicalDevice()
        {
            uint deviceCount = 0;
            vkEnumeratePhysicalDevices(myInstance, ref deviceCount, null);
            if (deviceCount == 0)
            {
                throw new InvalidOperationException("No physical devices exist.");
            }

            VkPhysicalDevice[] physicalDevices = new VkPhysicalDevice[deviceCount];
            vkEnumeratePhysicalDevices(myInstance, ref deviceCount, ref physicalDevices[0]);
            // Just use the first one.
            myPhysicalDevice = physicalDevices[0];

            vkGetPhysicalDeviceProperties(myPhysicalDevice, out myPhysicalDeviceProperties);
            string deviceName;
            fixed (byte* utf8NamePtr = myPhysicalDeviceProperties.deviceName)
            {
                deviceName = Encoding.UTF8.GetString(utf8NamePtr, (int)MaxPhysicalDeviceNameSize);
            }

            vkGetPhysicalDeviceFeatures(myPhysicalDevice, out myPhysicalDeviceFeatures);
            vkGetPhysicalDeviceMemoryProperties(myPhysicalDevice, out myPhysicalDeviceMemProperties);

            UInt32 queueFamilyCount = 0;
            vkGetPhysicalDeviceQueueFamilyProperties(myPhysicalDevice, ref queueFamilyCount, null);
            Info.print("\tFound {0} queue families", queueFamilyCount);

            VkQueueFamilyProperties[] queueFamilyProps = new VkQueueFamilyProperties[queueFamilyCount];
            vkGetPhysicalDeviceQueueFamilyProperties(myPhysicalDevice, ref queueFamilyCount, out queueFamilyProps[0]);

            bool presentFound = false;
            bool graphicsFound = false;
            bool computeFound = false;
            bool transferFound = false;
            bool sparseFound = false;

            for (UInt32 j = 0; j < queueFamilyProps.Length; j++)
            {
                Debug.print("\t\tQueue Family {0} has {1} queues with flags {2}", j, queueFamilyProps[j].queueCount, queueFamilyProps[j].queueFlags);
                VkBool32 supportsPresent;
                vkGetPhysicalDeviceSurfaceSupportKHR(myPhysicalDevice, j, mySurface, out supportsPresent);

                if (!presentFound && supportsPresent == true)
                {
                    Info.print("\tFound queue family supporting present: {0}", j);
                    myPresentQueueIndex = j;
                    presentFound = true;
                }

                if (!graphicsFound && queueFamilyProps[j].queueFlags.HasFlag(VkQueueFlags.Graphics))
                {
                    myGraphicsQueueIndex = j;
                    Info.print("\tFound queue family supporting graphics: {0}", j);
                    graphicsFound = true;
                }

                if (!transferFound && queueFamilyProps[j].queueFlags.HasFlag(VkQueueFlags.Transfer))
                {
                    myTransferQueueIndex = j;
                    Info.print("\tFound queue family supporting transfer: {0}", j);
                    transferFound = true;
                }

                if (!computeFound && queueFamilyProps[j].queueFlags.HasFlag(VkQueueFlags.Compute))
                {
                    myComputeQueueIndex = j;
                    Info.print("\tFound queue family supporting compute: {0}", j);
                    computeFound = true;
                }

                if (!sparseFound && queueFamilyProps[j].queueFlags.HasFlag(VkQueueFlags.SparseBinding))
                {
                    mySparseQueueIndex = j;
                    Info.print("\tFound queue family supporting sparse: {0}", j);
                    sparseFound = true;
                }
            }
        }

        void createSurface(IntPtr hInstance, IntPtr hwd)
        {
            Console.WriteLine("Creating surface");
            VkWin32SurfaceCreateInfoKHR info = VkWin32SurfaceCreateInfoKHR.New();
            info.flags = 0;
            info.hinstance = hInstance;
            info.hwnd = hwd;

            VkResult res = vkCreateWin32SurfaceKHR(myInstance, ref info, null, out mySurface);
            Util.checkResult(res, "Failed to create surface");
        }

        void createLogicalDevice()
        {
            HashSet<uint> familyIndices = new HashSet<uint> { myGraphicsQueueIndex, myPresentQueueIndex };
            VkDeviceQueueCreateInfo* queueCreateInfos = stackalloc VkDeviceQueueCreateInfo[familyIndices.Count];
            uint queueCreateInfosCount = (uint)familyIndices.Count;

            int i = 0;
            foreach (uint index in familyIndices)
            {
                VkDeviceQueueCreateInfo queueCreateInfo = VkDeviceQueueCreateInfo.New();
                queueCreateInfo.queueFamilyIndex = index;
                queueCreateInfo.queueCount = 1;
                float priority = 1f;
                queueCreateInfo.pQueuePriorities = &priority;
                queueCreateInfos[i] = queueCreateInfo;
                i += 1;
            }

            VkPhysicalDeviceFeatures deviceFeatures = new VkPhysicalDeviceFeatures();
            deviceFeatures.samplerAnisotropy = myPhysicalDeviceFeatures.samplerAnisotropy;
            deviceFeatures.fillModeNonSolid = myPhysicalDeviceFeatures.fillModeNonSolid;
            deviceFeatures.geometryShader = myPhysicalDeviceFeatures.geometryShader;
            deviceFeatures.depthClamp = myPhysicalDeviceFeatures.depthClamp;
            deviceFeatures.multiViewport = myPhysicalDeviceFeatures.multiViewport;
            deviceFeatures.textureCompressionBC = myPhysicalDeviceFeatures.textureCompressionBC;
            deviceFeatures.textureCompressionETC2 = myPhysicalDeviceFeatures.textureCompressionETC2;
            deviceFeatures.multiDrawIndirect = myPhysicalDeviceFeatures.multiDrawIndirect;
            deviceFeatures.drawIndirectFirstInstance = myPhysicalDeviceFeatures.drawIndirectFirstInstance;

            List<string> deviceExtensions = new List<string>(enumerateDeviceExtensions());
            if(deviceExtensions.Contains())

            {   
                string extensionName = Util.GetString(properties[property].extensionName);
                if (extensionName == "VK_EXT_debug_marker")
                {
                    extensionNames.Add(DeviceExtensions.VK_EXT_debug_marker);
                    _debugMarkerEnabled = true;
                }
                else if (extensionName == "VK_KHR_swapchain")
                {
                    extensionNames.Add((IntPtr)properties[property].extensionName);
                }
                else if (preferStandardClipY && extensionName == "VK_KHR_maintenance1")
                {
                    extensionNames.Add((IntPtr)properties[property].extensionName);
                    _standardClipYDirection = true;
                }
            }

            VkDeviceCreateInfo deviceCreateInfo = VkDeviceCreateInfo.New();
            deviceCreateInfo.queueCreateInfoCount = queueCreateInfosCount;
            deviceCreateInfo.pQueueCreateInfos = queueCreateInfos;

            deviceCreateInfo.pEnabledFeatures = &deviceFeatures;

            StackList<IntPtr> layerNames = new StackList<IntPtr>();
            if (_standardValidationSupported)
            {
                layerNames.Add(CommonStrings.StandardValidationLayerName);
            }
            deviceCreateInfo.enabledLayerCount = layerNames.Count;
            deviceCreateInfo.ppEnabledLayerNames = (byte**)layerNames.Data;

            deviceCreateInfo.enabledExtensionCount = extensionNames.Count;
            deviceCreateInfo.ppEnabledExtensionNames = (byte**)extensionNames.Data;

            result = vkCreateDevice(_physicalDevice, ref deviceCreateInfo, null, out _device);
            CheckResult(result);

            vkGetDeviceQueue(_device, _graphicsQueueIndex, 0, out _graphicsQueue);

            if (_debugMarkerEnabled)
            {
                IntPtr setObjectNamePtr;
                using (FixedUtf8String debugExtFnName = "vkDebugMarkerSetObjectNameEXT")
                {
                    setObjectNamePtr = vkGetInstanceProcAddr(_instance, debugExtFnName);
                }

                _setObjectNameDelegate = Marshal.GetDelegateForFunctionPointer<vkDebugMarkerSetObjectNameEXT_d>(setObjectNamePtr);
            }
        }

        #region enumerate instance/device layers and extensions
        string[] enumerateInstanceLayers()
        {
            Info.print("Enumerating Instance Layer Properties");
            uint propCount = 0;
            VkResult result = vkEnumerateInstanceLayerProperties(ref propCount, null);
            Util.checkResult(result, "Error getting instance layer count");

            if (propCount == 0)
            {
                return new string[0];
            }

            VkLayerProperties[] props = new VkLayerProperties[propCount];
            vkEnumerateInstanceLayerProperties(ref propCount, ref props[0]);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (byte* layerNamePtr = props[i].layerName)
                {
                    ret[i] = Util.GetString(layerNamePtr);
                    Info.print(ret[i]);
                }
            }

            return ret;
        }

        string[] enumerateInstanceExtensions()
        {
            Info.print("Enumerating Instance Extensions");
            uint propCount = 0;
            VkResult result = vkEnumerateInstanceExtensionProperties((byte*)null, ref propCount, null);
            Util.checkResult(result, "Error getting instance extension properties");

            if (propCount == 0)
            {
                return new string[0];
            }

            VkExtensionProperties[] props = new VkExtensionProperties[propCount];
            vkEnumerateInstanceExtensionProperties((byte*)null, ref propCount, ref props[0]);

            string[] ret = new string[propCount];
            for (int i = 0; i < propCount; i++)
            {
                fixed (byte* extensionNamePtr = props[i].extensionName)
                {
                    ret[i] = Util.GetString(extensionNamePtr);
                    Info.print(ret[i]);
                }
            }

            return ret;
        }

        string[] enumerateDeviceLayers()
        {
            Info.print("Enumerating Device Layer Properties");
            UInt32 count = 0;
            VkResult result = vkEnumerateDeviceLayerProperties(myPhysicalDevice, ref count, null);
            Util.checkResult(result, "Error getting device layer properties");

            if (count == 0)
            {
                return new string[0];
            }

            VkLayerProperties[] props = new VkLayerProperties[count];
            vkEnumerateDeviceLayerProperties(myPhysicalDevice, ref count, ref props[0]);

            string[] ret = new string[count];
            for (int i = 0; i < count; i++)
            {
                fixed (byte* layerNamePtr = props[i].layerName)
                {
                    ret[i] = Util.GetString(layerNamePtr);
                    Info.print(ret[i]);
                }
            }

            return ret;
        }

        string[] enumerateDeviceExtensions()
        {
            Debug.print("Enumerating Device Extensions");
            UInt32 count = 0;
            VkResult result = vkEnumerateDeviceExtensionProperties(myPhysicalDevice, (string)null, ref count, null);
            Util.checkResult(result, "Error getting device extension properties");

            VkExtensionProperties[] props = new VkExtensionProperties[count];
            vkEnumerateDeviceExtensionProperties(myPhysicalDevice, (string)null, ref count, ref props[0]);

            string[] ret = new string[count];
            for (int i = 0; i < count; i++)
            {
                fixed (byte* extensionNamePtr = props[i].extensionName)
                {
                    ret[i] = Util.GetString(extensionNamePtr);
                    Info.print(ret[i]);
                }
            }

            return ret;
        }
        #endregion


    }

}