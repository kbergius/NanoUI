using System.Runtime.InteropServices;

namespace StbTrueTypeSharp
{
	unsafe partial class StbTrueType
	{
		public static void* stbtt__hheap_alloc(stbtt__hheap* hh, ulong size, void* userdata)
		{
			if (hh->first_free != null)
			{
				var p = hh->first_free;
				hh->first_free = *(void**)p;
				return p;
			}

			if (hh->num_remaining_in_head_chunk == 0)
			{
				var count = size < 32 ? 2000 : size < 128 ? 800 : 100;
				var c = (stbtt__hheap_chunk*)CRuntime.malloc((ulong)sizeof(stbtt__hheap_chunk) +
															  size * (ulong)count);
				if (c == null)
					return null;
				c->next = hh->head;
				hh->head = c;
				hh->num_remaining_in_head_chunk = count;
			}

			--hh->num_remaining_in_head_chunk;
			return (sbyte*)hh->head + sizeof(stbtt__hheap_chunk) + size * (ulong)hh->num_remaining_in_head_chunk;
		}

		public static void stbtt__hheap_cleanup(stbtt__hheap* hh, void* userdata)
		{
			var c = hh->head;
			while (c != null)
			{
				var n = c->next;
				CRuntime.free(c);
				c = n;
			}
		}

		public static void stbtt__hheap_free(stbtt__hheap* hh, void* p)
		{
			*(void**)p = hh->first_free;
			hh->first_free = p;
		}

		public static stbtt__active_edge* stbtt__new_active(stbtt__hheap* hh, stbtt__edge* e, int off_x,
			float start_point, void* userdata)
		{
			var z = (stbtt__active_edge*)stbtt__hheap_alloc(hh, (ulong)sizeof(stbtt__active_edge), userdata);
			var dxdy = (e->x1 - e->x0) / (e->y1 - e->y0);
			if (z == null)
				return z;
			z->fdx = dxdy;
			z->fdy = dxdy != 0.0f ? 1.0f / dxdy : 0.0f;
			z->fx = e->x0 + dxdy * (start_point - e->y0);
			z->fx -= off_x;
			z->direction = e->invert != 0 ? 1.0f : -1.0f;
			z->sy = e->y0;
			z->ey = e->y1;
			z->next = null;
			return z;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__hheap
		{
			public stbtt__hheap_chunk* head;
			public void* first_free;
			public int num_remaining_in_head_chunk;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__hheap_chunk
		{
			public stbtt__hheap_chunk* next;
		}
	}
}