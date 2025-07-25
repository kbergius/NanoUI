using System.Runtime.InteropServices;

namespace StbTrueTypeSharp
{
	unsafe partial class StbTrueType
	{
		public static void stbtt__csctx_close_shape(stbtt__csctx* ctx)
		{
			if (ctx->first_x != ctx->x || ctx->first_y != ctx->y)
				stbtt__csctx_v(ctx, STBTT_vline, (int)ctx->first_x, (int)ctx->first_y, 0, 0, 0, 0);
		}

		public static void stbtt__csctx_rccurve_to(stbtt__csctx* ctx, float dx1, float dy1, float dx2, float dy2,
			float dx3, float dy3)
		{
			var cx1 = ctx->x + dx1;
			var cy1 = ctx->y + dy1;
			var cx2 = cx1 + dx2;
			var cy2 = cy1 + dy2;
			ctx->x = cx2 + dx3;
			ctx->y = cy2 + dy3;
			stbtt__csctx_v(ctx, STBTT_vcubic, (int)ctx->x, (int)ctx->y, (int)cx1, (int)cy1, (int)cx2, (int)cy2);
		}

		public static void stbtt__csctx_rline_to(stbtt__csctx* ctx, float dx, float dy)
		{
			ctx->x += dx;
			ctx->y += dy;
			stbtt__csctx_v(ctx, STBTT_vline, (int)ctx->x, (int)ctx->y, 0, 0, 0, 0);
		}

		public static void stbtt__csctx_rmove_to(stbtt__csctx* ctx, float dx, float dy)
		{
			stbtt__csctx_close_shape(ctx);
			ctx->first_x = ctx->x = ctx->x + dx;
			ctx->first_y = ctx->y = ctx->y + dy;
			stbtt__csctx_v(ctx, STBTT_vmove, (int)ctx->x, (int)ctx->y, 0, 0, 0, 0);
		}

		public static void stbtt__csctx_v(stbtt__csctx* c, byte type, int x, int y, int cx, int cy, int cx1, int cy1)
		{
			if (c->bounds != 0)
			{
				stbtt__track_vertex(c, x, y);
				if (type == STBTT_vcubic)
				{
					stbtt__track_vertex(c, cx, cy);
					stbtt__track_vertex(c, cx1, cy1);
				}
			}
			else
			{
				stbtt_setvertex(&c->pvertices[c->num_vertices], type, x, y, cx, cy);
				c->pvertices[c->num_vertices].cx1 = (short)cx1;
				c->pvertices[c->num_vertices].cy1 = (short)cy1;
			}

			c->num_vertices++;
		}

		public static void stbtt__track_vertex(stbtt__csctx* c, int x, int y)
		{
			if (x > c->max_x || c->started == 0)
				c->max_x = x;
			if (y > c->max_y || c->started == 0)
				c->max_y = y;
			if (x < c->min_x || c->started == 0)
				c->min_x = x;
			if (y < c->min_y || c->started == 0)
				c->min_y = y;
			c->started = 1;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__csctx
		{
			public int bounds;
			public int started;
			public float first_x;
			public float first_y;
			public float x;
			public float y;
			public int min_x;
			public int max_x;
			public int min_y;
			public int max_y;
			public stbtt_vertex* pvertices;
			public int num_vertices;
		}
	}
}