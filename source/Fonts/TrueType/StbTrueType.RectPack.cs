using System.Runtime.InteropServices;

namespace StbTrueTypeSharp
{
	unsafe partial class StbTrueType
	{
		public static void stbrp_init_target(stbrp_context* con, int pw, int ph, stbrp_node* nodes, int num_nodes)
		{
			con->width = pw;
			con->height = ph;
			con->x = 0;
			con->y = 0;
			con->bottom_y = 0;
		}

		public static void stbrp_pack_rects(stbrp_context* con, stbrp_rect* rects, int num_rects)
		{
			var i = 0;
			for (i = 0; i < num_rects; ++i)
			{
				if (con->x + rects[i].w > con->width)
				{
					con->x = 0;
					con->y = con->bottom_y;
				}

				if (con->y + rects[i].h > con->height)
					break;
				rects[i].x = con->x;
				rects[i].y = con->y;
				rects[i].was_packed = 1;
				con->x += rects[i].w;
				if (con->y + rects[i].h > con->bottom_y)
					con->bottom_y = con->y + rects[i].h;
			}

			for (; i < num_rects; ++i) rects[i].was_packed = 0;
		}

		public static int stbtt_PackBegin(stbtt_pack_context spc, byte* pixels, int pw, int ph, int stride_in_bytes,
			int padding, void* alloc_context)
		{
			var context = (stbrp_context*)CRuntime.malloc((ulong)sizeof(stbrp_context));
			var num_nodes = pw - padding;
			var nodes = (stbrp_node*)CRuntime.malloc((ulong)(sizeof(stbrp_node) * num_nodes));
			if (context == null || nodes == null)
			{
				if (context != null)
					CRuntime.free(context);
				if (nodes != null)
					CRuntime.free(nodes);
				return 0;
			}

			spc.user_allocator_context = alloc_context;
			spc.width = pw;
			spc.height = ph;
			spc.pixels = pixels;
			spc.pack_info = context;
			spc.nodes = nodes;
			spc.padding = padding;
			spc.stride_in_bytes = stride_in_bytes != 0 ? stride_in_bytes : pw;
			spc.h_oversample = 1;
			spc.v_oversample = 1;
			spc.skip_missing = 0;
			stbrp_init_target(context, pw - padding, ph - padding, nodes, num_nodes);
			if (pixels != null)
				CRuntime.memset(pixels, 0, (ulong)(pw * ph));
			return 1;
		}

		public static void stbtt_PackEnd(stbtt_pack_context spc)
		{
			CRuntime.free(spc.nodes);
			CRuntime.free(spc.pack_info);
		}

		public static int stbtt_PackFontRange(stbtt_pack_context spc, byte* fontdata, int font_index, float font_size,
			int first_unicode_codepoint_in_range, int num_chars_in_range, stbtt_packedchar* chardata_for_range)
		{
			var range = new stbtt_pack_range();
			range.first_unicode_codepoint_in_range = first_unicode_codepoint_in_range;
			range.array_of_unicode_codepoints = null;
			range.num_chars = num_chars_in_range;
			range.chardata_for_range = chardata_for_range;
			range.font_size = font_size;
			return stbtt_PackFontRanges(spc, fontdata, font_index, &range, 1);
		}

		public static int stbtt_PackFontRanges(stbtt_pack_context spc, byte* fontdata, int font_index,
			stbtt_pack_range* ranges, int num_ranges)
		{
			var info = new stbtt_fontinfo();
			var i = 0;
			var j = 0;
			var n = 0;
			var return_value = 1;
			stbrp_rect* rects;
			for (i = 0; i < num_ranges; ++i)
				for (j = 0; j < ranges[i].num_chars; ++j)
					ranges[i].chardata_for_range[j].x0 = ranges[i].chardata_for_range[j].y0 =
						ranges[i].chardata_for_range[j].x1 = ranges[i].chardata_for_range[j].y1 = 0;

			n = 0;
			for (i = 0; i < num_ranges; ++i) n += ranges[i].num_chars;

			rects = (stbrp_rect*)CRuntime.malloc((ulong)(sizeof(stbrp_rect) * n));
			if (rects == null)
				return 0;
			info.userdata = spc.user_allocator_context;
			stbtt_InitFont(info, fontdata, stbtt_GetFontOffsetForIndex(fontdata, font_index));
			n = stbtt_PackFontRangesGatherRects(spc, info, ranges, num_ranges, rects);
			stbtt_PackFontRangesPackRects(spc, rects, n);
			return_value = stbtt_PackFontRangesRenderIntoRects(spc, info, ranges, num_ranges, rects);
			CRuntime.free(rects);
			return return_value;
		}

		public static int stbtt_PackFontRangesGatherRects(stbtt_pack_context spc, stbtt_fontinfo info,
			stbtt_pack_range* ranges, int num_ranges, stbrp_rect* rects)
		{
			var i = 0;
			var j = 0;
			var k = 0;
			var missing_glyph_added = 0;
			k = 0;
			for (i = 0; i < num_ranges; ++i)
			{
				var fh = ranges[i].font_size;
				var scale = fh > 0 ? stbtt_ScaleForPixelHeight(info, fh) : stbtt_ScaleForMappingEmToPixels(info, -fh);
				ranges[i].h_oversample = (byte)spc.h_oversample;
				ranges[i].v_oversample = (byte)spc.v_oversample;
				for (j = 0; j < ranges[i].num_chars; ++j)
				{
					var x0 = 0;
					var y0 = 0;
					var x1 = 0;
					var y1 = 0;
					var codepoint = ranges[i].array_of_unicode_codepoints == null
						? ranges[i].first_unicode_codepoint_in_range + j
						: ranges[i].array_of_unicode_codepoints[j];
					var glyph = stbtt_FindGlyphIndex(info, codepoint);
					if (glyph == 0 && (spc.skip_missing != 0 || missing_glyph_added != 0))
					{
						rects[k].w = rects[k].h = 0;
					}
					else
					{
						stbtt_GetGlyphBitmapBoxSubpixel(info, glyph, scale * spc.h_oversample, scale * spc.v_oversample,
							0, 0, &x0, &y0, &x1, &y1);
						rects[k].w = (int)(x1 - x0 + spc.padding + spc.h_oversample - 1);
						rects[k].h = (int)(y1 - y0 + spc.padding + spc.v_oversample - 1);
						if (glyph == 0)
							missing_glyph_added = 1;
					}

					++k;
				}
			}

			return k;
		}

		public static void stbtt_PackFontRangesPackRects(stbtt_pack_context spc, stbrp_rect* rects, int num_rects)
		{
			stbrp_pack_rects((stbrp_context*)spc.pack_info, rects, num_rects);
		}

		public static int stbtt_PackFontRangesRenderIntoRects(stbtt_pack_context spc, stbtt_fontinfo info,
			stbtt_pack_range* ranges, int num_ranges, stbrp_rect* rects)
		{
			var i = 0;
			var j = 0;
			var k = 0;
			var missing_glyph = -1;
			var return_value = 1;
			var old_h_over = (int)spc.h_oversample;
			var old_v_over = (int)spc.v_oversample;
			k = 0;
			for (i = 0; i < num_ranges; ++i)
			{
				var fh = ranges[i].font_size;
				var scale = fh > 0 ? stbtt_ScaleForPixelHeight(info, fh) : stbtt_ScaleForMappingEmToPixels(info, -fh);
				float recip_h = 0;
				float recip_v = 0;
				float sub_x = 0;
				float sub_y = 0;
				spc.h_oversample = ranges[i].h_oversample;
				spc.v_oversample = ranges[i].v_oversample;
				recip_h = 1.0f / spc.h_oversample;
				recip_v = 1.0f / spc.v_oversample;
				sub_x = stbtt__oversample_shift((int)spc.h_oversample);
				sub_y = stbtt__oversample_shift((int)spc.v_oversample);
				for (j = 0; j < ranges[i].num_chars; ++j)
				{
					var r = &rects[k];
					if (r->was_packed != 0 && r->w != 0 && r->h != 0)
					{
						var bc = &ranges[i].chardata_for_range[j];
						var advance = 0;
						var lsb = 0;
						var x0 = 0;
						var y0 = 0;
						var x1 = 0;
						var y1 = 0;
						var codepoint = ranges[i].array_of_unicode_codepoints == null
							? ranges[i].first_unicode_codepoint_in_range + j
							: ranges[i].array_of_unicode_codepoints[j];
						var glyph = stbtt_FindGlyphIndex(info, codepoint);
						var pad = spc.padding;
						r->x += pad;
						r->y += pad;
						r->w -= pad;
						r->h -= pad;
						stbtt_GetGlyphHMetrics(info, glyph, &advance, &lsb);
						stbtt_GetGlyphBitmapBox(info, glyph, scale * spc.h_oversample, scale * spc.v_oversample, &x0,
							&y0, &x1, &y1);
						stbtt_MakeGlyphBitmapSubpixel(info, spc.pixels + r->x + r->y * spc.stride_in_bytes,
							(int)(r->w - spc.h_oversample + 1), (int)(r->h - spc.v_oversample + 1),
							spc.stride_in_bytes, scale * spc.h_oversample, scale * spc.v_oversample, 0, 0, glyph);
						if (spc.h_oversample > 1)
							stbtt__h_prefilter(spc.pixels + r->x + r->y * spc.stride_in_bytes, r->w, r->h,
								spc.stride_in_bytes, spc.h_oversample);
						if (spc.v_oversample > 1)
							stbtt__v_prefilter(spc.pixels + r->x + r->y * spc.stride_in_bytes, r->w, r->h,
								spc.stride_in_bytes, spc.v_oversample);
						bc->x0 = (ushort)(short)r->x;
						bc->y0 = (ushort)(short)r->y;
						bc->x1 = (ushort)(short)(r->x + r->w);
						bc->y1 = (ushort)(short)(r->y + r->h);
						bc->xadvance = scale * advance;
						bc->xoff = x0 * recip_h + sub_x;
						bc->yoff = y0 * recip_v + sub_y;
						bc->xoff2 = (x0 + r->w) * recip_h + sub_x;
						bc->yoff2 = (y0 + r->h) * recip_v + sub_y;
						if (glyph == 0)
							missing_glyph = j;
					}
					else if (spc.skip_missing != 0)
					{
						return_value = 0;
					}
					else if (r->was_packed != 0 && r->w == 0 && r->h == 0 && missing_glyph >= 0)
					{
						ranges[i].chardata_for_range[j] = ranges[i].chardata_for_range[missing_glyph];
					}
					else
					{
						return_value = 0;
					}

					++k;
				}
			}

			spc.h_oversample = (uint)old_h_over;
			spc.v_oversample = (uint)old_v_over;
			return return_value;
		}

		public static void stbtt_PackSetOversampling(stbtt_pack_context spc, uint h_oversample, uint v_oversample)
		{
			if (h_oversample <= 8)
				spc.h_oversample = h_oversample;
			if (v_oversample <= 8)
				spc.v_oversample = v_oversample;
		}

		public static void stbtt_PackSetSkipMissingCodepoints(stbtt_pack_context spc, int skip)
		{
			spc.skip_missing = skip;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbrp_context
		{
			public int width;
			public int height;
			public int x;
			public int y;
			public int bottom_y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbrp_node
		{
			public byte x;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbrp_rect
		{
			public int x;
			public int y;
			public int id;
			public int w;
			public int h;
			public int was_packed;
		}

		public class stbtt_pack_context
		{
			public uint h_oversample;
			public int height;
			public void* nodes;
			public void* pack_info;
			public int padding;
			public byte* pixels;
			public int skip_missing;
			public int stride_in_bytes;
			public void* user_allocator_context;
			public uint v_oversample;
			public int width;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt_pack_range
		{
			public float font_size;
			public int first_unicode_codepoint_in_range;
			public int* array_of_unicode_codepoints;
			public int num_chars;
			public stbtt_packedchar* chardata_for_range;
			public byte h_oversample;
			public byte v_oversample;
		}
	}
}