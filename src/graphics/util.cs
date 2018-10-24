using System;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;


using Vulkan;
using static Vulkan.NativeLibrary;

namespace Graphics
{
    public static class Util
    {
        [Conditional("DEBUG")]
        public static void checkResult(VkResult result, string error)
        {
            if (result != VkResult.Success)
            {
                throw new Exception(String.Format("{0} : {1} ", error, result));
            }
        }

        public static unsafe string GetString(byte* stringStart)
        {
            int characters = 0;
            while (stringStart[characters] != 0)
            {
                characters++;
            }

            return Encoding.UTF8.GetString(stringStart, characters);
        }
    }


    internal struct VkVersion
    {
        private readonly uint value;

        public VkVersion(uint major, uint minor, uint patch)
        {
            value = major << 22 | minor << 12 | patch;
        }

        public uint Major => value >> 22;
        public uint Minor => (value >> 12) & 0x3ff;
        public uint Patch => (value >> 22) & 0xfff;

        public static implicit operator uint(VkVersion version)
        {
            return version.value;
        }
    }

    internal unsafe class FixedUtf8String : IDisposable
    {
        private GCHandle _handle;
        private uint _numBytes;

        public byte* StringPtr => (byte*)_handle.AddrOfPinnedObject().ToPointer();

        public FixedUtf8String(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s));
            }

            int byteCount = Encoding.UTF8.GetByteCount(s);
            byte[] text = new byte[byteCount + 1];
            _handle = GCHandle.Alloc(text, GCHandleType.Pinned);
            _numBytes = (uint)text.Length - 1; // Includes null terminator
            int encodedCount = Encoding.UTF8.GetBytes(s, 0, s.Length, text, 0);
            Debug.Assert(encodedCount == byteCount);
        }

        private string GetString()
        {
            return Encoding.UTF8.GetString(StringPtr, (int)_numBytes);
        }

        public void Dispose()
        {
            _handle.Free();
        }

        public override string ToString() => GetString();

        public static implicit operator byte* (FixedUtf8String utf8String) => utf8String.StringPtr;
        public static implicit operator IntPtr(FixedUtf8String utf8String) => new IntPtr(utf8String.StringPtr);
        public static implicit operator FixedUtf8String(string s) => new FixedUtf8String(s);
        public static implicit operator string(FixedUtf8String utf8String) => utf8String.GetString();
    }

    internal unsafe class FixedUtf8StringArray : IDisposable
    {
        IntPtr* myPtr;
        int myCount;

        public FixedUtf8StringArray(List<string> data)
        {
            myCount = data.Count;
            myPtr = (IntPtr*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)) * data.Count);
            for (int i = 0; i < data.Count; i++)
            {
                myPtr[i] = Marshal.StringToHGlobalAnsi(data[i]);
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < myCount; i++)
            {
                Marshal.FreeHGlobal(myPtr[i]);
            }

            Marshal.FreeHGlobal(new IntPtr(myPtr));
        }

        public byte** ptrPtr => (byte**)(myPtr);

        public static implicit operator byte** (FixedUtf8StringArray utf8StringArray) => utf8StringArray.ptrPtr;
    }

    #region Extension and Layer Names
    public partial class InstanceExtensions
    {
        public const string VK_KHR_android_surface = "VK_KHR_android_surface";
        public const string VK_KHR_display = "VK_KHR_display";
        public const string VK_KHR_external_fence_capabilities = "VK_KHR_external_fence_capabilities";
        public const string VK_KHR_external_memory_capabilities = "VK_KHR_external_memory_capabilities";
        public const string VK_KHR_external_semaphore_capabilities = "VK_KHR_external_semaphore_capabilities";
        public const string VK_KHR_get_physical_device_properties2 = "VK_KHR_get_physical_device_properties2";
        public const string VK_KHR_get_surface_capabilities2 = "VK_KHR_get_surface_capabilities2";
        public const string VK_KHR_mir_surface = "VK_KHR_mir_surface";
        public const string VK_KHR_surface = "VK_KHR_surface";
        public const string VK_KHR_wayland_surface = "VK_KHR_wayland_surface";
        public const string VK_KHR_win32_surface = "VK_KHR_win32_surface";
        public const string VK_KHR_xcb_surface = "VK_KHR_xcb_surface";
        public const string VK_KHR_xlib_surface = "VK_KHR_xlib_surface";

        public const string VK_KHX_device_group_creation = "VK_KHX_device_group_creation";

        public const string VK_EXT_acquire_xlib_display = "VK_EXT_acquire_xlib_display";
        public const string VK_EXT_debug_report = "VK_EXT_debug_report";
        public const string VK_EXT_direct_mode_display = "VK_EXT_direct_mode_display";
        public const string VK_EXT_display_surface_counter = "VK_EXT_display_surface_counter";
        public const string VK_EXT_swapchain_colorspace = "VK_EXT_swapchain_colorspace";
        public const string VK_EXT_validation_flags = "VK_EXT_validation_flags";

        public const string VK_MVK_ios_surface = "VK_MVK_ios_surface";
        public const string VK_MVK_macos_surface = "VK_MVK_macos_surface";

        public const string VK_NN_vi_surface = "VK_NN_vi_surface";

        public const string VK_NV_external_memory_capabilities = "VK_NV_external_memory_capabilities";
    }

    public partial class InstanceLayers
    {
        public const string VK_LAYER_LUNARG_standard_validation = "VK_LAYER_LUNARG_standard_validation";
    }

    public partial class DeviceExtensions
    {
        public const string VK_KHR_16bit_storage = "VK_KHR_16bit_storage";
        public const string VK_KHR_descriptor_update_template = "VK_KHR_descriptor_update_template";
        public const string VK_KHR_dedicated_allocation = "VK_KHR_dedicated_allocation";
        public const string VK_KHR_display_swapchain = "VK_KHR_display_swapchain";
        public const string VK_KHR_external_fence = "VK_KHR_external_fence";
        public const string VK_KHR_external_fence_fd = "VK_KHR_external_fence_fd";
        public const string VK_KHR_external_fence_win32 = "VK_KHR_external_fence_win32";
        public const string VK_KHR_external_memory = "VK_KHR_external_memory";
        public const string VK_KHR_external_memory_fd = "VK_KHR_external_memory_fd";
        public const string VK_KHR_external_memory_win32 = "VK_KHR_external_memory_win32";
        public const string VK_KHR_external_semaphore = "VK_KHR_external_semaphore";
        public const string VK_KHR_external_semaphore_fd = "VK_KHR_external_semaphore_fd";
        public const string VK_KHR_external_semaphore_win32 = "VK_KHR_external_semaphore_win32";
        public const string VK_KHR_get_memory_requirements2 = "VK_KHR_get_memory_requirements2";
        public const string VK_KHR_incremental_present = "VK_KHR_incremental_present";
        public const string VK_KHR_maintenance1 = "VK_KHR_maintenance1";
        public const string VK_KHR_push_descriptor = "VK_KHR_push_descriptor";
        public const string VK_KHR_sampler_mirror_clamp_to_edge = "VK_KHR_sampler_mirror_clamp_to_edge";
        public const string VK_KHR_shader_draw_parameters = "VK_KHR_shader_draw_parameters";
        public const string VK_KHR_shared_presentable_image = "VK_KHR_shared_presentable_image";
        public const string VK_KHR_storage_buffer_storage_class = "VK_KHR_storage_buffer_storage_class";
        public const string VK_KHR_swapchain = "VK_KHR_swapchain";
        public const string VK_KHR_variable_pointers = "VK_KHR_variable_pointers";
        public const string VK_KHR_win32_keyed_mutex = "VK_KHR_win32_keyed_mutex";

        public const string VK_KHX_device_group = "VK_KHX_device_group";
        public const string VK_KHX_multiview = "VK_KHX_multiview";

        public const string VK_EXT_blend_operation_advanced = "VK_EXT_blend_operation_advanced";
        public const string VK_EXT_debug_marker = "VK_EXT_debug_marker";
        public const string VK_EXT_discard_rectangles = "VK_EXT_discard_rectangles";
        public const string VK_EXT_display_control = "VK_EXT_display_control";
        public const string VK_EXT_hdr_metadata = "VK_EXT_hdr_metadata";
        public const string VK_EXT_sampler_filter_minmax = "VK_EXT_sampler_filter_minmax";
        public const string VK_EXT_shader_subgroup_ballot = "VK_EXT_shader_subgroup_ballot";
        public const string VK_EXT_shader_subgroup_vote = "VK_EXT_shader_subgroup_vote";

        public const string VK_AMD_draw_indirect_count = "VK_AMD_draw_indirect_count";
        public const string VK_AMD_gcn_shader = "VK_AMD_gcn_shader";
        public const string VK_AMD_gpu_shader_half_float = "VK_AMD_gpu_shader_half_float";
        public const string VK_AMD_gpu_shader_int16 = "VK_AMD_gpu_shader_int16";
        public const string VK_AMD_negative_viewport_height = "VK_AMD_negative_viewport_height";
        public const string VK_AMD_rasterization_order = "VK_AMD_rasterization_order";
        public const string VK_AMD_shader_ballot = "VK_AMD_shader_ballot";
        public const string VK_AMD_shader_explicit_vertex_parameter = "VK_AMD_shader_explicit_vertex_parameter";
        public const string VK_AMD_shader_trinary_minmax = "VK_AMD_shader_trinary_minmax";
        public const string VK_AMD_texture_gather_bias_lod = "VK_AMD_texture_gather_bias_lod";

        public const string VK_GOOGLE_display_timing = "VK_GOOGLE_display_timing";

        public const string VK_IMG_filter_cubic = "VK_IMG_filter_cubic";

        public const string VK_NV_clip_space_w_scaling = "VK_NV_clip_space_w_scaling";
        public const string VK_NV_dedicated_allocation = "VK_NV_dedicated_allocation";
        public const string VK_NV_external_memory = "VK_NV_external_memory";
        public const string VK_NV_external_memory_win32 = "VK_NV_external_memory_win32";
        public const string VK_NV_fill_rectangle = "VK_NV_fill_rectangle";
        public const string VK_NV_fragment_coverage_to_color = "VK_NV_fragment_coverage_to_color";
        public const string VK_NV_framebuffer_mixed_samples = "VK_NV_framebuffer_mixed_samples";
        public const string VK_NV_geometry_shader_passthrough = "VK_NV_geometry_shader_passthrough";
        public const string VK_NV_glsl_shader = "VK_NV_glsl_shader";
        public const string VK_NV_sample_mask_override_coverage = "VK_NV_sample_mask_override_coverage";
        public const string VK_NV_viewport_array2 = "VK_NV_viewport_array2";
        public const string VK_NV_viewport_swizzle = "VK_NV_viewport_swizzle";
        public const string VK_NV_win32_keyed_mutex = "VK_NV_win32_keyed_mutex";

        public const string VK_NVX_device_generated_commands = "VK_NVX_device_generated_commands";
        public const string VK_NVX_multiview_per_view_attributes = "VK_NVX_multiview_per_view_attributes";
    }
    #endregion
}