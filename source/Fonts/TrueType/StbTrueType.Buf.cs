using System.Runtime.InteropServices;

namespace StbTrueTypeSharp
{
	unsafe partial class StbTrueType
	{
		public static uint stbtt__buf_get(stbtt__buf* b, int n)
		{
			uint v = 0;
			var i = 0;
			for (i = 0; i < n; i++) v = (v << 8) | stbtt__buf_get8(b);

			return v;
		}

		public static byte stbtt__buf_get8(stbtt__buf* b)
		{
			if (b->cursor >= b->size)
				return 0;
			return b->data[b->cursor++];
		}

		public static byte stbtt__buf_peek8(stbtt__buf* b)
		{
			if (b->cursor >= b->size)
				return 0;
			return b->data[b->cursor];
		}

		public static stbtt__buf stbtt__buf_range(stbtt__buf* b, int o, int s)
		{
			var r = stbtt__new_buf(null, 0);
			if (o < 0 || s < 0 || o > b->size || s > b->size - o)
				return r;
			r.data = b->data + o;
			r.size = s;
			return r;
		}

		public static void stbtt__buf_seek(stbtt__buf* b, int o)
		{
			b->cursor = o > b->size || o < 0 ? b->size : o;
		}

		public static void stbtt__buf_skip(stbtt__buf* b, int o)
		{
			stbtt__buf_seek(b, b->cursor + o);
		}

		public static stbtt__buf stbtt__cff_get_index(stbtt__buf* b)
		{
			var count = 0;
			var start = 0;
			var offsize = 0;
			start = b->cursor;
			count = (int)stbtt__buf_get(b, 2);
			if (count != 0)
			{
				offsize = stbtt__buf_get8(b);
				stbtt__buf_skip(b, offsize * count);
				stbtt__buf_skip(b, (int)(stbtt__buf_get(b, offsize) - 1));
			}

			return stbtt__buf_range(b, start, b->cursor - start);
		}

		public static int stbtt__cff_index_count(stbtt__buf* b)
		{
			stbtt__buf_seek(b, 0);
			return (int)stbtt__buf_get(b, 2);
		}

		public static stbtt__buf stbtt__cff_index_get(stbtt__buf b, int i)
		{
			var count = 0;
			var offsize = 0;
			var start = 0;
			var end = 0;
			stbtt__buf_seek(&b, 0);
			count = (int)stbtt__buf_get(&b, 2);
			offsize = stbtt__buf_get8(&b);
			stbtt__buf_skip(&b, i * offsize);
			start = (int)stbtt__buf_get(&b, offsize);
			end = (int)stbtt__buf_get(&b, offsize);
			return stbtt__buf_range(&b, 2 + (count + 1) * offsize + start, end - start);
		}

		public static uint stbtt__cff_int(stbtt__buf* b)
		{
			int b0 = stbtt__buf_get8(b);
			if (b0 >= 32 && b0 <= 246)
				return (uint)(b0 - 139);
			if (b0 >= 247 && b0 <= 250)
				return (uint)((b0 - 247) * 256 + stbtt__buf_get8(b) + 108);
			if (b0 >= 251 && b0 <= 254)
				return (uint)(-(b0 - 251) * 256 - stbtt__buf_get8(b) - 108);
			if (b0 == 28)
				return stbtt__buf_get(b, 2);
			if (b0 == 29)
				return stbtt__buf_get(b, 4);
			return 0;
		}

		public static void stbtt__cff_skip_operand(stbtt__buf* b)
		{
			var v = 0;
			int b0 = stbtt__buf_peek8(b);
			if (b0 == 30)
			{
				stbtt__buf_skip(b, 1);
				while (b->cursor < b->size)
				{
					v = stbtt__buf_get8(b);
					if ((v & 0xF) == 0xF || v >> 4 == 0xF)
						break;
				}
			}
			else
			{
				stbtt__cff_int(b);
			}
		}

		public static stbtt__buf stbtt__dict_get(stbtt__buf* b, int key)
		{
			stbtt__buf_seek(b, 0);
			while (b->cursor < b->size)
			{
				var start = b->cursor;
				var end = 0;
				var op = 0;
				while (stbtt__buf_peek8(b) >= 28) stbtt__cff_skip_operand(b);

				end = b->cursor;
				op = stbtt__buf_get8(b);
				if (op == 12)
					op = stbtt__buf_get8(b) | 0x100;
				if (op == key)
					return stbtt__buf_range(b, start, end - start);
			}

			return stbtt__buf_range(b, 0, 0);
		}

		public static void stbtt__dict_get_ints(stbtt__buf* b, int key, int outcount, uint* _out_)
		{
			var i = 0;
			var operands = stbtt__dict_get(b, key);
			for (i = 0; i < outcount && operands.cursor < operands.size; i++) _out_[i] = stbtt__cff_int(&operands);
		}

		public static stbtt__buf stbtt__get_subr(stbtt__buf idx, int n)
		{
			var count = stbtt__cff_index_count(&idx);
			var bias = 107;
			if (count >= 33900)
				bias = 32768;
			else if (count >= 1240)
				bias = 1131;
			n += bias;
			if (n < 0 || n >= count)
				return stbtt__new_buf(null, 0);
			return stbtt__cff_index_get(idx, n);
		}

		public static stbtt__buf stbtt__get_subrs(stbtt__buf cff, stbtt__buf fontdict)
		{
			uint subrsoff = 0;
			var private_loc = stackalloc uint[] { 0, 0 };
			var pdict = new stbtt__buf();
			stbtt__dict_get_ints(&fontdict, 18, 2, private_loc);
			if (private_loc[1] == 0 || private_loc[0] == 0)
				return stbtt__new_buf(null, 0);
			pdict = stbtt__buf_range(&cff, (int)private_loc[1], (int)private_loc[0]);
			stbtt__dict_get_ints(&pdict, 19, 1, &subrsoff);
			if (subrsoff == 0)
				return stbtt__new_buf(null, 0);
			stbtt__buf_seek(&cff, (int)(private_loc[1] + subrsoff));
			return stbtt__cff_get_index(&cff);
		}

		public static stbtt__buf stbtt__new_buf(void* p, ulong size)
		{
			var r = new stbtt__buf();
			r.data = (byte*)p;
			r.size = (int)size;
			r.cursor = 0;
			return r;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbtt__buf
		{
			public byte* data;
			public int cursor;
			public int size;
		}
	}
}