using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PsypherLibrary.SupportLibrary.Utils
{
    public static class Texture2dUtilities
    {
        private struct PointsRegion
        {
            public int direction;
            public int xLeft;
            public int xRight;
            public int y;

            public override string ToString()
            {
                return ("(direction=" + direction + ",x[" + xLeft + "->" + xRight + "] y=" + y + ")");
            }
        }

        static int originalXLeft, originalXRigth;
        static bool[,] resultBoolRegion;

        public static bool[,] floodFillLineGetRegion(Vector2 point, Color32[] colors, bool[] persistentLayer, int width, int height)
        {
            //go to left and to the right
            // if down pixel vector has border get righ pixel to bottomQueue
            // if upper pixel has unvisited node


            if (resultBoolRegion == null || resultBoolRegion.GetLongLength(0) != width || resultBoolRegion.GetLength(1) != height)
            {
                resultBoolRegion = new bool[width, height];
            }
            else
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        resultBoolRegion[i, j] = false;
                    }
                }
            }

            Color32 seedColor = colors[(int) ((point.y * width) + point.x)];
            PointsRegion initial = new PointsRegion();
            initial.xLeft = (int) point.x;
            initial.xRight = (int) point.x;
            initial.y = (int) point.y;
            initial.direction = 1;

            Queue queue = new Queue();
            queue.Enqueue(initial);

            scanLeftPixelsForBound(colors, ref seedColor, persistentLayer, ref initial, ref width);
            scanRightPixelsForBound(width, colors, ref seedColor, persistentLayer, ref initial, ref width);

            scanUpBelowPixelsRegionBackward(ref initial.xLeft, ref initial.xRight, ref initial.y, ref height, ref initial,
                ref colors, ref persistentLayer, ref seedColor, ref queue, resultBoolRegion, ref width);


            while (queue.Count > 0)
            {
                PointsRegion pointsRegion = (PointsRegion) queue.Dequeue();

                if (isPointsRegionVisited(pointsRegion, resultBoolRegion))
                    continue;


                originalXLeft = pointsRegion.xLeft - 1;
                scanLeftPixelsForBound(colors, ref seedColor, persistentLayer, ref pointsRegion, ref width);
                if (originalXLeft > pointsRegion.xLeft)
                    scanUpBelowPixelsRegionBackward(ref pointsRegion.xLeft, ref originalXLeft, ref pointsRegion.y, ref height, ref pointsRegion,
                        ref colors, ref persistentLayer, ref seedColor, ref queue, resultBoolRegion, ref width);

                originalXRigth = pointsRegion.xRight + 1;
                scanRightPixelsForBound(width, colors, ref seedColor, persistentLayer, ref pointsRegion, ref width);
                if (originalXRigth < pointsRegion.xRight)
                    scanUpBelowPixelsRegionBackward(ref originalXRigth, ref pointsRegion.xRight, ref pointsRegion.y, ref height, ref pointsRegion,
                        ref colors, ref persistentLayer, ref seedColor, ref queue, resultBoolRegion, ref width);

                for (int xx = pointsRegion.xLeft; xx <= pointsRegion.xRight; xx++)
                {
                    resultBoolRegion[xx, pointsRegion.y] = true;
                }

                // 2. get DownPixel  -this is not exactly down pixel (it depends of direction) 
                scanUpBelowPixelsRegionForward(ref pointsRegion.xLeft, ref pointsRegion.xRight, ref pointsRegion.y, ref height, ref pointsRegion,
                    ref colors, ref persistentLayer, ref seedColor, ref queue, resultBoolRegion, ref width);
            }


            return resultBoolRegion;
        }

        static void scanLeftPixelsForBound(Color32[] colors, ref Color32 seedColor, bool[] persistentLayer, ref PointsRegion pointsRegion, ref int width)
        {
            for (int xx = pointsRegion.xLeft; xx >= 0; xx--)
            {
                if (isPixelSeed(ref xx, ref pointsRegion.y, ref seedColor, ref colors, ref persistentLayer, ref width))
                {
                    pointsRegion.xLeft = xx;
                }
                else
                {
                    break;
                }
            }
        }

        static void scanRightPixelsForBound(int workspaceWidth, Color32[] colors, ref Color32 seedColor, bool[] persistentLayer, ref PointsRegion pointsRegion, ref int width)
        {
            for (int xx = pointsRegion.xRight; xx < workspaceWidth; xx++)
            {
                if (isPixelSeed(ref xx, ref pointsRegion.y, ref seedColor, ref colors, ref persistentLayer, ref width))
                {
                    pointsRegion.xRight = xx;
                }
                else
                {
                    break;
                }
            }
        }

        static bool isPointsRegionVisited(PointsRegion pointsRegion, bool[,] resultBoolRegion)
        {
            return resultBoolRegion[pointsRegion.xLeft, pointsRegion.y];
        }

        //this function scan upper or below pixels diapason, and if it find new seedpixel regions add it to queue
        static PointsRegion __newPointsRegion;
        static int __yy, __xx, i31;
        static bool __prevPixelSeed;

        private static void scanUpBelowPixelsRegionForward(ref int xLeft, ref int xRight, ref int baseY, ref int maxY, ref PointsRegion pointsRegion,
            ref Color32[] colors, ref bool[] persistentBorder, ref Color32 seedColor, ref Queue queue, bool[,] resultRegion, ref int width)
        {
            __newPointsRegion = new PointsRegion();

            __yy = baseY + pointsRegion.direction;
            if (__yy >= 0 && __yy < maxY)
            {
                __prevPixelSeed = false;
                for (__xx = xLeft; __xx <= xRight; __xx++)
                {
                    i31 = __yy * width + __xx;
                    if (resultRegion[__xx, __yy] != true
                        && (!persistentBorder[i31] //is pixel seed start
                            && ((colors[i31].a < 255)
                                || (seedColor.r == colors[i31].r
                                    && seedColor.g == colors[i31].g
                                    && seedColor.b == colors[i31].b
                                    && seedColor.a == colors[i31].a))) //is pixel seed end
                    )
                    {
                        if (!__prevPixelSeed)
                        {
                            __newPointsRegion.direction = pointsRegion.direction;
                            __newPointsRegion.y = __yy;
                            __newPointsRegion.xLeft = __xx;
                            __prevPixelSeed = true;
                        }
                    }
                    else
                    {
                        if (__prevPixelSeed)
                        {
                            __newPointsRegion.xRight = __xx - 1;
                            __prevPixelSeed = false;
                            queue.Enqueue(__newPointsRegion);
                        }
                    }
                }

                if (__prevPixelSeed)
                {
                    __newPointsRegion.xRight = xRight;
                    queue.Enqueue(__newPointsRegion);
                }
            }
        }

        static int i32;

        private static void scanUpBelowPixelsRegionBackward(ref int xLeft, ref int xRight, ref int baseY, ref int maxY, ref PointsRegion pointsRegion,
            ref Color32[] colors, ref bool[] persistentBorder, ref Color32 seedColor, ref Queue queue, bool[,] resultRegion, ref int width)
        {
            __newPointsRegion = new PointsRegion();

            __yy = baseY - pointsRegion.direction;
            if (__yy >= 0 && __yy < maxY)
            {
                __prevPixelSeed = false;
                for (__xx = xLeft; __xx <= xRight; __xx++)
                {
                    i32 = __yy * width + __xx;
                    if (resultRegion[__xx, __yy] != true
                        && (!persistentBorder[i32] // is pixel seed start
                            && ((colors[i32].a < 255)
                                || (seedColor.r == colors[i32].r
                                    && seedColor.g == colors[i32].g
                                    && seedColor.b == colors[i32].b
                                    && seedColor.a == colors[i32].a))) //is pixel seed end
                    )
                    {
                        if (!__prevPixelSeed)
                        {
                            __newPointsRegion.direction = -pointsRegion.direction;
                            __newPointsRegion.y = __yy;
                            __newPointsRegion.xLeft = __xx;
                            __prevPixelSeed = true;
                        }
                    }
                    else
                    {
                        if (__prevPixelSeed)
                        {
                            __newPointsRegion.xRight = __xx - 1;
                            __prevPixelSeed = false;
                            queue.Enqueue(__newPointsRegion);
                        }
                    }
                }

                if (__prevPixelSeed)
                {
                    __newPointsRegion.xRight = xRight;
                    queue.Enqueue(__newPointsRegion);
                }
            }
        }


        static int i33;

        private static bool isPixelSeed(ref int x, ref int y, ref Color32 seedColor, ref Color32[] colors, ref bool[] persistentBorder, ref int width)
        {
            i33 = y * width + x;
            if (persistentBorder[i33])
                return false;

            if (colors[i33].a < 255)
                return true;

            if (seedColor.r != colors[i33].r
                || seedColor.g != colors[i33].g
                || seedColor.b != colors[i33].b
                || seedColor.a != colors[i33].a)
                return false;
            return true;
        }

        private static byte byteAbs(byte a)
        {
            return (byte) ((a < 0) ? -a : a);
        }

        public static bool ColorEquals(Color a, Color b, float sensetivity)
        {
            if (Mathf.Abs(a[1] - b[1]) < sensetivity &&
                Mathf.Abs(a[2] - b[2]) < sensetivity &&
                Mathf.Abs(a[3] - b[3]) < sensetivity)
                return true;
            return false;
        }

        public static void DoFloodFill(ref Texture2D source, ref Color32[] originalColors, Vector2 position, ref bool[] outLineLayer, Color32 colorToApply)
        {
            var width = source.width;
            var height = source.height;
            if (outLineLayer[(int) ((position.y * width) + position.x)])
            {
                Debug.Log("Black Region");
                return;
            }

            bool[,] resultRegion = floodFillLineGetRegion(position, originalColors, outLineLayer, width, height);
            Color32 activeColor = colorToApply;
            int tCounter = 0;
            for (int yy = 0; yy < height; yy++)
            {
                for (int xx = 0; xx < width; xx++)
                {
                    if (resultRegion[xx, yy])
                    {
                        originalColors[tCounter].r = activeColor.r;
                        originalColors[tCounter].g = activeColor.g;
                        originalColors[tCounter].b = activeColor.b;
                        originalColors[tCounter].a = 255;
                    }

                    tCounter++;
                }
            }

            source.SetPixels32(originalColors);
            source.Apply();
        }

        public static void GetDominantColor(ref Texture2D source, ref Color32[] colors, out Color32 dominantColor, bool useSample = false)
        {
            dominantColor = new Color32();
            List<Color32> colorPalette = new List<Color32>()
            {
                new Color32(0, 0, 0, 0), //black 
                new Color32(128, 128, 128, 255), //gray 
                new Color32(192, 192, 192, 255), //silver 
                new Color32(255, 255, 255, 255), //white 
                new Color32(128, 0, 0, 255), //maroon 
                new Color32(255, 0, 0, 255), //red 
                new Color32(128, 128, 0, 255), //olive 
                new Color32(255, 255, 0, 255), //yellow
                new Color32(0, 128, 0, 255), //green 
                new Color32(0, 255, 0, 255), //lime 
                new Color32(0, 128, 128, 255), //teal 
                new Color32(0, 255, 255, 255), //aqua 
                new Color32(0, 0, 128, 255), //navy 
                new Color32(0, 0, 255, 255), //blue 
                new Color32(128, 0, 128, 255), //purple 
                new Color32(255, 0, 255, 255), //fuchsia 
                new Color32(255, 165, 0, 255), //orange
            };
            Dictionary<Color32, int> colorRatio = new Dictionary<Color32, int>();
            if (useSample)
            {
                for (int i = 0; i < colors.Length; i += 10)
                {
                    if (colors[i].a > 0)
                    {
                        for (int j = 0; j < colorPalette.Count; j++)
                        {
                            if (ColorEquals(colors[i], colorPalette[j], 0.5f))
                            {
                                if (colorRatio.ContainsKey(colorPalette[j]))
                                {
                                    colorRatio[colorPalette[j]]++;
                                }
                                else
                                {
                                    colorRatio.Add(colorPalette[j], 1);
                                }
                            }
                        }
                    }
                }
            }

            else
            {
                for (int i = 0; i < colors.Length; i += 10)
                {
                    if (colors[i].a > 0)
                    {
                        if (colorRatio.ContainsKey(colors[i]))
                        {
                            colorRatio[colors[i]]++;
                        }
                        else
                        {
                            colorRatio.Add(colors[i], 1);
                        }
                    }
                }
            }

            var domColor = colorRatio.OrderByDescending(x => x.Value).First();
            dominantColor = domColor.Key;
        }

        public static Texture2D AlphaBlend(this Texture2D aBottom, Texture2D aTop)
        {
            if (aBottom.width != aTop.width || aBottom.height != aTop.height)
                throw new System.InvalidOperationException("AlphaBlend only works with two equal sized images");
            var bData = aBottom.GetPixels();
            var tData = aTop.GetPixels();
            int count = bData.Length;
            var rData = new Color[count];
            for (int i = 0; i < count; i++)
            {
                Color B = bData[i];
                Color T = tData[i];
                float srcF = T.a;
                float destF = 1f - T.a;
                float alpha = srcF + destF * B.a;
                Color R = (T * srcF + B * B.a * destF) / alpha;
                R.a = alpha;
                rData[i] = R;
            }

            var res = new Texture2D(aTop.width, aTop.height, TextureFormat.ARGB32, true);
            res.SetPixels(rData);
            res.Apply();
            return res;
        }
    }
}