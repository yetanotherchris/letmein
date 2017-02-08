namespace Letmein.Core.Services.UniqueId.PronounceablePassword
{
	public class GpwData
	{
		public virtual long Sigma
		{
			get
			{
				return sigma[0];
			}
		}

		public static short[][][] tris = null;
		public static long[] sigma = null; // 125729

		public GpwData()
		{
			int c1, c2, c3;
			tris = new short[26][][];
			for (int i = 0; i < 26; i++)
			{
				tris[i] = new short[26][];
				for (int i2 = 0; i2 < 26; i2++)
				{
					tris[i][i2] = new short[26];
				}
			}
			sigma = new long[1];
			GpwDataInit1.fill(this); // Break into two classes for NS 4.0
			GpwDataInit2.Fill(this); // .. its Java 1.1 barfs on methods > 65K
			for (c1 = 0; c1 < 26; c1++)
			{
				for (c2 = 0; c2 < 26; c2++)
				{
					for (c3 = 0; c3 < 26; c3++)
					{
						sigma[0] += (long)tris[c1][c2][c3];
					} // for c3
				} // for c2
			} // for c1
		} // constructor

		public virtual void set_Renamed(int x1, int x2, int x3, short v)
		{
			tris[x1][x2][x3] = v;
		} // set()

		public virtual long get_Renamed(int x1, int x2, int x3)
		{
			return (long)tris[x1][x2][x3];
		} // get()
	}
}