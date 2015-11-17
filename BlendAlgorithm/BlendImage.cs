namespace BlendAlgorithm
{
    public class BlendImage
    {
        /// <summary>
        /// Ważone nakładanie dwóch obrazów.
        /// </summary>
        /// <param name="imgBottom">referencja do obrazu na, który zostanie nałożony imgTop</param>
        /// <param name="imgTop">obraz nakładany</param>
        /// <param name="alpha">waga obrazu nakładanego (od 0 do 255)</param>
        /// <param name="start">definiuje indeks początkowy od którego 
        /// wątek będzie wykoywał obliczenia</param>
        /// <param name="stop">
        /// definiuje ineks końcowy na którym wątek zatrzyma obliczenia</param>
      //  public static void blendToImages(byte[] imgBottom, byte[] imgTop, float alpha, int start, int stop)
        public static void blendToImages(byte[][]bitmaps, int[]coords, int alpha)
        {
            // ustawienie kanału alfa (przeźroczystości)
            float alphaTop = (float)alpha / 255.0F;
            float alphaBottom = 1.0F - alphaTop;

            /*
             * Realizacja ważonego dodawania obrazu przy zastosowaniu wzoru postaci
             * Image(x,y) = W*Image1(x,y) +(1-W)*Image2(x,y), gdzie 0 ≤ W ≤ 1 
             */
            for (int j = coords[0] * 4; j < coords[1]* 4; )
            {
                bitmaps[0][j] = (byte)((alphaBottom * (float)bitmaps[0][j]) +
                                (alphaTop * (float)bitmaps[1][j]));
                ++j;
                bitmaps[0][j] = (byte)((alphaBottom * (float)bitmaps[0][j]) +
                                (alphaTop * (float)bitmaps[1][j]));
                ++j;
                bitmaps[0][j] = (byte)((alphaBottom * (float)bitmaps[0][j]) +
                                (alphaTop * (float)bitmaps[1][j]));
                ++j;
                bitmaps[0][j] = 255;
                ++j;
            }
        }
    }
}
