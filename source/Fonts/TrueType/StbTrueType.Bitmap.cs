using System.Runtime.InteropServices;

namespace StbTrueTypeSharp
{
	unsafe partial class StbTrueType
	{
		public static void stbtt__fill_active_edges_new(float* scanline, float* scanline_fill, int len,
			stbtt__active_edge* e, float y_top)
		{
			var y_bottom = y_top + 1;
			while (e != null)
			{
				if (e->fdx == 0)
				{
					var x0 = e->fx;
					if (x0 < len)
					{
						if (x0 >= 0)
						{
							stbtt__handle_clipped_edge(scanline, (int)x0, e, x0, y_top, x0, y_bottom);
							stbtt__handle_clipped_edge(scanline_fill - 1, (int)x0 + 1, e, x0, y_top, x0, y_bottom);
						}
						else
						{
							stbtt__handle_clipped_edge(scanline_fill - 1, 0, e, x0, y_top, x0, y_bottom);
						}
					}
				}
				else
				{
					var x0 = e->fx;
					var dx = e->fdx;
					var xb = x0 + dx;
					float x_top = 0;
					float x_bottom = 0;
					float sy0 = 0;
					float sy1 = 0;
					var dy = e->fdy;
					if (e->sy > y_top)
					{
						x_top = x0 + dx * (e->sy - y_top);
						sy0 = e->sy;
					}
					else
					{
						x_top = x0;
						sy0 = y_top;
					}

					if (e->ey < y_bottom)
					{
						x_bottom = x0 + dx * (e->ey - y_top);
						sy1 = e->ey;
					}
					else
					{
						x_bottom = xb;
						sy1 = y_bottom;
					}

					if (x_top >= 0 && x_bottom >= 0 && x_top < len && x_bottom < len)
					{
						if ((int)x_top == (int)x_bottom)
						{
							float height = 0;
							var x = (int)x_top;
							height = (sy1 - sy0) * e->direction;
							scanline[x] += stbtt__position_trapezoid_area(height, x_top, x + 1.0f, x_bottom, x + 1.0f);
							scanline_fill[x] += height;
						}
						else
						{
							var x = 0;
							var x1 = 0;
							var x2 = 0;
							float y_crossing = 0;
							float y_final = 0;
							float step = 0;
							float sign = 0;
							float area = 0;
							if (x_top > x_bottom)
							{
								float t = 0;
								sy0 = y_bottom - (sy0 - y_top);
								sy1 = y_bottom - (sy1 - y_top);
								t = sy0;
								sy0 = sy1;
								sy1 = t;
								t = x_bottom;
								x_bottom = x_top;
								x_top = t;
								dx = -dx;
								dy = -dy;
								t = x0;
								x0 = xb;
								xb = t;
							}

							x1 = (int)x_top;
							x2 = (int)x_bottom;
							y_crossing = y_top + dy * (x1 + 1 - x0);
							y_final = y_top + dy * (x2 - x0);
							if (y_crossing > y_bottom)
								y_crossing = y_bottom;
							sign = e->direction;
							area = sign * (y_crossing - sy0);
							scanline[x1] += stbtt__sized_triangle_area(area, x1 + 1 - x_top);
							if (y_final > y_bottom)
							{
								y_final = y_bottom;
								dy = (y_final - y_crossing) / (x2 - (x1 + 1));
							}

							step = sign * dy * 1;
							for (x = x1 + 1; x < x2; ++x)
							{
								scanline[x] += area + step / 2;
								area += step;
							}

							scanline[x2] += area + sign *
								stbtt__position_trapezoid_area(sy1 - y_final, x2, x2 + 1.0f, x_bottom, x2 + 1.0f);
							scanline_fill[x2] += sign * (sy1 - sy0);
						}
					}
					else
					{
						var x = 0;
						for (x = 0; x < len; ++x)
						{
							var y0 = y_top;
							float x1 = x;
							float x2 = x + 1;
							var x3 = xb;
							var y3 = y_bottom;
							var y1 = (x - x0) / dx + y_top;
							var y2 = (x + 1 - x0) / dx + y_top;
							if (x0 < x1 && x3 > x2)
							{
								stbtt__handle_clipped_edge(scanline, x, e, x0, y0, x1, y1);
								stbtt__handle_clipped_edge(scanline, x, e, x1, y1, x2, y2);
								stbtt__handle_clipped_edge(scanline, x, e, x2, y2, x3, y3);
							}
							else if (x3 < x1 && x0 > x2)
							{
								stbtt__handle_clipped_edge(scanline, x, e, x0, y0, x2, y2);
								stbtt__handle_clipped_edge(scanline, x, e, x2, y2, x1, y1);
								stbtt__handle_clipped_edge(scanline, x, e, x1, y1, x3, y3);
							}
							else if (x0 < x1 && x3 > x1)
							{
								stbtt__handle_clipped_edge(scanline, x, e, x0, y0, x1, y1);
								stbtt__handle_clipped_edge(scanline, x, e, x1, y1, x3, y3);
							}
							else if (x3 < x1 && x0 > x1)
							{
								stbtt__handle_clipped_edge(scanline, x, e, x0, y0, x1, y1);
								stbtt__handle_clipped_edge(scanline, x, e, x1, y1, x3, y3);
							}
							else if (x0 < x2 && x3 > x2)
							{
								stbtt__handle_clipped_edge(scanline, x, e, x0, y0, x2, y2);
								stbtt__handle_clipped_edge(scanline, x, e, x2, y2, x3, y3);
							}
							else if (x3 < x2 && x0 > x2)
							{
								stbtt__handle_clipped_edge(scanline, x, e, x0, y0, x2, y2);
								stbtt__handle_clipped_edge(scanline, x, e, x2, y2, x3, y3);
							}
							else
							{
								stbtt__handle_clipped_edge(scanline, x, e, x0, y0, x3, y3);
							}
						}
					}
				}

				e = e->next;
			}
		}

		public static void stbtt__handle_clipped_edge(float* scanline, int x, stbtt__active_edge* e, float x0, float y0,
			float x1, float y1)
		{
			if (y0 == y1)
				return;
			if (y0 > e->ey)
				return;
			if (y1 < e->sy)
				return;
			if (y0 < e->sy)
			{
				x0 += (x1 - x0) * (e->sy - y0) / (y1 - y0);
				y0 = e->sy;
			}

			if (y1 > e->ey)
			{
				x1 += (x1 - x0) * (e->ey - y1) / (y1 - y0);
				y1 = e->ey;
			}

			if (x0 <= x && x1 <= x)
			{
				scanline[x] += e->direction * (y1 - y0);
			}
			else if (x0 >= x + 1 && x1 >= x + 1)
			{
			}
			else
			{
				scanline[x] += e->direction * (y1 - y0) * (1 - (x0 - x + (x1 - x)) / 2);
			}
		}

		public static void stbtt__rasterize(stbtt__bitmap* result, stbtt__point* pts, int* wcount, int windings,
			float scale_x, float scale_y, float shift_x, float shift_y, int off_x, int off_y, int invert,
			void* userdata, bool useOldRasterizer)
		{
			var y_scale_inv = invert != 0 ? -scale_y : scale_y;
			stbtt__edge* e;
			var n = 0;
			var i = 0;
			var j = 0;
			var k = 0;
			var m = 0;

			var vsubsample = 1;
			if (useOldRasterizer)
			{
				vsubsample = (result->h) < (8) ? 15 : 5;
			}

			n = 0;
			for (i = 0; i < windings; ++i) n += wcount[i];

			e = (stbtt__edge*)CRuntime.malloc((ulong)(sizeof(stbtt__edge) * (n + 1)));
			if (e == null)
				return;
			n = 0;
			m = 0;
			for (i = 0; i < windings; ++i)
			{
				var p = pts + m;
				m += wcount[i];
				j = wcount[i] - 1;
				for (k = 0; k < wcount[i]; j = k++)
				{
					var a = k;
					var b = j;
					if (p[j].y == p[k].y)
						continue;
					e[n].invert = 0;
					if (invert != 0 && p[j].y > p[k].y ||
						invert == 0 && p[j].y < p[k].y)
					{
						e[n].invert = 1;
						a = j;
						b = k;
					}

					e[n].x0 = p[a].x * scale_x + shift_x;
					e[n].y0 = (p[a].y * y_scale_inv + shift_y) * vsubsample;
					e[n].x1 = p[b].x * scale_x + shift_x;
					e[n].y1 = (p[b].y * y_scale_inv + shift_y) * vsubsample;
					++n;
				}
			}

			stbtt__sort_edges(e, n);

			if (!useOldRasterizer)
			{
				stbtt__rasterize_sorted_edges(result, e, n, vsubsample, off_x, off_y, userdata);
			} else
			{
				stbtt__rasterize_sorted_edges_old_rasterizer(result, e, n, vsubsample, off_x, off_y, userdata);
			}

			CRuntime.free(e);
		}

		public static void stbtt__rasterize_sorted_edges(stbtt__bitmap* result, stbtt__edge* e, int n, int vsubsample,
			int off_x, int off_y, void* userdata)
		{
			usedOldRasterizer = false;

			var hh = new stbtt__hheap();
			stbtt__active_edge* active = null;
			var y = 0;
			var j = 0;
			var i = 0;
			var scanline_data = stackalloc float[129];
			float* scanline;
			float* scanline2;
			if (result->w > 64)
				scanline = (float*)CRuntime.malloc((ulong)((result->w * 2 + 1) * sizeof(float)));
			else
				scanline = scanline_data;
			scanline2 = scanline + result->w;
			y = off_y;
			e[n].y0 = (float)(off_y + result->h) + 1;
			while (j < result->h)
			{
				var scan_y_top = y + 0.0f;
				var scan_y_bottom = y + 1.0f;
				var step = &active;
				CRuntime.memset(scanline, 0, (ulong)(result->w * sizeof(float)));
				CRuntime.memset(scanline2, 0, (ulong)((result->w + 1) * sizeof(float)));
				while (*step != null)
				{
					var z = *step;
					if (z->ey <= scan_y_top)
					{
						*step = z->next;
						z->direction = 0;
						stbtt__hheap_free(&hh, z);
					}
					else
					{
						step = &(*step)->next;
					}
				}

				while (e->y0 <= scan_y_bottom)
				{
					if (e->y0 != e->y1)
					{
						var z = stbtt__new_active(&hh, e, off_x, scan_y_top, userdata);
						if (z != null)
						{
							if (j == 0 && off_y != 0)
								if (z->ey < scan_y_top)
									z->ey = scan_y_top;

							z->next = active;
							active = z;
						}
					}

					++e;
				}

				if (active != null)
					stbtt__fill_active_edges_new(scanline, scanline2 + 1, result->w, active, scan_y_top);
				{
					float sum = 0;
					for (i = 0; i < result->w; ++i)
					{
						float k = 0;
						var m = 0;
						sum += scanline2[i];
						k = scanline[i] + sum;
						k = CRuntime.fabs(k) * 255 + 0.5f;
						m = (int)k;
						if (m > 255)
							m = 255;
						result->pixels[j * result->stride + i] = (byte)m;
					}
				}

				step = &active;
				while (*step != null)
				{
					var z = *step;
					z->fx += z->fdx;
					step = &(*step)->next;
				}

				++y;
				++j;
			}

			stbtt__hheap_cleanup(&hh, userdata);
			if (scanline != scanline_data)
				CRuntime.free(scanline);
		}

		public static void stbtt__sort_edges(stbtt__edge* p, int n)
		{
			stbtt__sort_edges_quicksort(p, n);
			stbtt__sort_edges_ins_sort(p, n);
		}

		public static void stbtt__sort_edges_ins_sort(stbtt__edge* p, int n)
		{
			var i = 0;
			var j = 0;
			for (i = 1; i < n; ++i)
			{
				var t = p[i];
				var a = &t;
				j = i;
				while (j > 0)
				{
					var b = &p[j - 1];
					var c = a->y0 < b->y0 ? 1 : 0;
					if (c == 0)
						break;
					p[j] = p[j - 1];
					--j;
				}

				if (i != j)
					p[j] = t;
			}
		}

		public static void stbtt__sort_edges_quicksort(stbtt__edge* p, int n)
		{
			while (n > 12)
			{
				var t = new stbtt__edge();
				var c01 = 0;
				var c12 = 0;
				var c = 0;
				var m = 0;
				var i = 0;
				var j = 0;
				m = n >> 1;
				c01 = (&p[0])->y0 < (&p[m])->y0 ? 1 : 0;
				c12 = (&p[m])->y0 < (&p[n - 1])->y0 ? 1 : 0;
				if (c01 != c12)
				{
					var z = 0;
					c = (&p[0])->y0 < (&p[n - 1])->y0 ? 1 : 0;
					z = c == c12 ? 0 : n - 1;
					t = p[z];
					p[z] = p[m];
					p[m] = t;
				}

				t = p[0];
				p[0] = p[m];
				p[m] = t;
				i = 1;
				j = n - 1;
				for (; ; )
				{
					for (; ; ++i)
						if (!((&p[i])->y0 < (&p[0])->y0))
							break;

					for (; ; --j)
						if (!((&p[0])->y0 < (&p[j])->y0))
							break;

					if (i >= j)
						break;
					t = p[i];
					p[i] = p[j];
					p[j] = t;
					++i;
					--j;
				}

				if (j < n - i)
				{
					stbtt__sort_edges_quicksort(p, j);
					p = p + i;
					n = n - i;
				}
				else
				{
					stbtt__sort_edges_quicksort(p + i, n - i);
					n = j;
				}
			}
		}

		public static void stbtt_Rasterize(stbtt__bitmap* result, float flatness_in_pixels, stbtt_vertex* vertices,
			int num_verts, float scale_x, float scale_y, float shift_x, float shift_y, int x_off, int y_off, int invert,
			void* userdata, bool useOldRasterizer)
		{
			var scale = scale_x > scale_y ? scale_y : scale_x;
			var winding_count = 0;
			int* winding_lengths = null;
			var windings = stbtt_FlattenCurves(vertices, num_verts, flatness_in_pixels / scale, &winding_lengths,
				&winding_count, userdata);
			if (windings != null)
			{
				stbtt__rasterize(result, windings, winding_lengths, winding_count, scale_x, scale_y, shift_x, shift_y,
					x_off, y_off, (int)invert, userdata, useOldRasterizer);
				CRuntime.free(winding_lengths);
				CRuntime.free(windings);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__active_edge
		{
			public stbtt__active_edge* next;
			public float fx;
			public float fdx;
			public float fdy;
			public float direction;
			public float sy;
			public float ey;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__bitmap
		{
			public int w;
			public int h;
			public int stride;
			public byte* pixels;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__edge
		{
			public float x0;
			public float y0;
			public float x1;
			public float y1;
			public int invert;
		}
	}
}