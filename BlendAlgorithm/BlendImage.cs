
namespace BlendAlgorithm
{
    public class BlendImage
    {
        public static void blendToImages(byte[] imgBottom, byte[] imgTop, float alpha, int start, int stop)
        {
            // ustawienie kanału alfa (przeźroczystości)
            float alphaTop = (float)alpha / 255.0F;
            float alphaBottom = 1.0F - alphaTop;

            /**
             * Realizacja ważonego dodawania obrazu przy zastosowaniu wzoru postaci
             * Image(x,y) = W*Image1(x,y) +(1-W)*Image2(x,y), gdzie 0 ≤ W ≤ 1 
            */
            for (int j = start * 4; j < stop * 4; )
            {
                imgBottom[j] = (byte)((alphaBottom * (float)imgBottom[j]) +
                                (alphaTop * (float)imgTop[j]));
                ++j;
                imgBottom[j] = (byte)((alphaBottom * (float)imgBottom[j]) +
                                (alphaTop * (float)imgTop[j]));
                ++j;
                imgBottom[j] = (byte)((alphaBottom * (float)imgBottom[j]) +
                                (alphaTop * (float)imgTop[j]));
                ++j;
                imgBottom[j] = 255;
                ++j;
            }
        }
    }
}
