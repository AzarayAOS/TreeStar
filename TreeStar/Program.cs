using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace TreeStar
{
    internal class Program
    {
        /// <summary>
        /// Класс на одну звезду с его идентификатором и звёздной величиной
        /// </summary>
        public class Star
        {
            public Star(int id, double ra, double dec, double mgt, double x, double y, double z)
            {
                Id = id;
                Ra = ra;
                Dec = dec;
                Mgt = mgt;
                X = x;
                Y = y;
                Z = z;
            }

            public Star(int id, double ra, double dec, double mgt)
            {
                Id = id;
                Ra = ra;
                Dec = dec;
                Mgt = mgt;
                CalculateXYZ();
            }

            public object[] GetObj()
            {
                return new object[] { Id, Ra, Dec, Mgt, X, Y, Z };
            }

            public Vector3 GetXYZ()
            {
                return new Vector3(
                    Convert.ToSingle(X),
                    Convert.ToSingle(Y),
                    Convert.ToSingle(Z));
            }

            public int Id { get; set; }
            public double Ra { get; set; }
            public double Dec { get; set; }
            public double Mgt { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Z { get; set; }

            private float CosD(double degry) => Convert.ToSingle(Math.Cos(Math.PI / 180 * degry));

            private float SinD(double degry) => Convert.ToSingle(Math.Sin(Math.PI / 180 * degry));

            public void CalculateXYZ()
            {
                X = CosD(Dec) * CosD(Ra);
                Y = CosD(Dec) * SinD(Ra);
                Z = SinD(Dec);
            }
        }

        /// <summary>
        /// Класс треугольников
        /// </summary>
        public class Triangles
        {
            public Triangles(int hip1, int hiparac2, int hiparac3, double ang1, double ang2, double phi)
            {
                HipID1 = hip1;
                HipID2 = hiparac2;
                HipID3 = hiparac3;
                Theta1 = ang1;
                Theta2 = ang2;
                Phi = phi;
            }

            public int HipID1 { get; set; }
            public int HipID2 { get; set; }
            public int HipID3 { get; set; }

            public double Theta1 { get; set; }
            public double Theta2 { get; set; }
            public double Phi { get; set; }

            public string ToString(string sep)
            {
                return HipID1.ToString() + sep +
                       HipID2.ToString() + sep +
                       HipID3.ToString() + sep +
                       Theta1.ToString() + sep +
                       Theta2.ToString() + sep +
                       Phi.ToString();
            }
        }

        public class Pattern
        {
            public Pattern(int spot1, int spot2, int spot3, double theta1, double theta2, double phi)
            {
                Spot1 = spot1;
                Spot2 = spot2;
                Spot3 = spot3;
                Theta1 = theta1;
                Theta2 = theta2;
                Phi = phi;
            }

            public int Spot1 { get; set; }
            public int Spot2 { get; set; }
            public int Spot3 { get; set; }

            public double Theta1 { get; set; }
            public double Theta2 { get; set; }
            public double Phi { get; set; }
        }

        public class StarID
        {
            public StarID(int votes, int spot, double hipID, Vector3 xYZ)
            {
                Votes = votes;
                Spot = spot;
                HipID = hipID;
                XYZ = xYZ;
            }

            public StarID(int votes, int spot, double hipID, double x, double y, double z)
            {
                Votes = votes;
                Spot = spot;
                HipID = hipID;
                XYZ = new Vector3(
                    Convert.ToSingle(x),
                    Convert.ToSingle(y),
                    Convert.ToSingle(z));
            }

            public StarID(int spot, double x, double y, double z)
            {
                Spot = spot;
                XYZ = new Vector3(
                    Convert.ToSingle(x),
                    Convert.ToSingle(y),
                    Convert.ToSingle(z));
            }

            public StarID(int spot, Vector3 xyz)
            {
                Spot = spot;
                XYZ = xyz;
            }

            public int Votes { get; set; }
            public int Spot { get; set; }

            public double HipID { get; set; }
            public Vector3 XYZ { get; set; }
        }

        private static void Main(string[] args)
        {
            string FileCatalog="";      // путь к файлу каталога
            string FileScreen="";       // путь к файлу с RA и DEC снимка

            Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];

            double Ra;
            double Dec;
            double Mg=6;

            double FOV=20;

            List<Star>CatalogStar= CreateCatalogTriad(Mg, FileCatalog, separator);      // загружаем каталог звёзд с определёнными зв.величинами
            List<Triangles>featurelist2=Triad_Feature_Extract(Mg,FOV,CatalogStar);      //
        }

        /// <summary>
        /// Чтение каталога <paramref name="datacatalog" /> и занесение всех звёзд с з. величиной
        /// равной и меньше <paramref name="Mg" />
        /// </summary>
        public static List<Star> CreateCatalogTriad(double Mg, string datacatalog, Char separator)
        {
            List<Star> CatalogStar = new List<Star>();
            CatalogStar.Add(new Star(-1, -1, -1, -1));  // заглушка, чтоб всё было с единицы

            using(StreamReader sr = new StreamReader(datacatalog, System.Text.Encoding.Default))
            {
                string line;
                line = sr.ReadLine();
                //int i = 0;
                while((line = sr.ReadLine()) != null)
                {
                    string[] ch = line.Split(";");

                    int id = Convert.ToInt32(ch[0]);
                    float ra = Convert.ToSingle(ch[1].Replace('.', separator));
                    float dec = Convert.ToSingle(ch[2].Replace('.', separator));
                    float mgt = Convert.ToSingle(ch[3].Replace('.', separator));
                    //float x = Convert.ToSingle(ch[4].Replace('.', separator));
                    //float y = Convert.ToSingle(ch[5].Replace('.', separator));
                    //float z = Convert.ToSingle(ch[6].Replace('.', separator));

                    //CatalogStar.Rows.Add(new Star(id, ra, dec, mgt, x, y, z).GetObj());
                    if(mgt <= Mg)
                        CatalogStar.Add((new Star(id, ra, dec, mgt)));
                }
            }
            Console.WriteLine("Каталог прочитан!");
            return CatalogStar;
        }

        /// <summary>
        /// Функция С.Братта для создания каталога триадных признаков на основе вычисленной звездной
        /// величины отсечки. Создает список углов точечного произведения между звездой и следующими
        /// двумя самыми близкими звездами.Также находит интерьер точечного угла между этими тремя звездами.
        /// </summary>
        public static List<Triangles> Triad_Feature_Extract(double Mg, double FOV, List<Star> catalog)
        {
            List<Triangles> feat=new List<Triangles>();
            feat.Add(new Triangles(-1, -1, -1, -1, -1, -1));    // заглушка, чтоб начиналос с 1го индекса

            Vector3 A;
            Vector3 B;
            Vector3 C;

            Vector3 Vec1,Vec2;

            int Hip1,Hip2,Hip3;
            int Hiparc2, Hiparc3;

            double theta, theta1,theta2;

            Vector3 NewXYZ;

            double v1,v2;

            double phi;

            double ang1,ang2;

            for(int j = 1; j <= catalog.Count; j++)
            {
                A = catalog[j].GetXYZ();    // Желаемый вектор для сравнения
                B = C = Vector3.Zero;

                Hip1 = catalog[j].Id;
                Hip2 = Hip3 = -1;

                theta1 = theta2 = 360;

                for(int i = 1; i <= catalog.Count; i++)
                {
                    //Получить 2й вектор
                    NewXYZ = catalog[i].GetXYZ();
                    if(!NewXYZ.Equals(A))
                    {
                        // Найти угол между нужным вектором и вторым вектором
                        theta = Math.Acos(Vector3.Dot(A, NewXYZ));

                        //Сравнить и обновить углы
                        if(theta1 < theta && theta < theta2)
                        {
                            theta2 = theta;
                            Hip3 = catalog[i].Id;
                            C = NewXYZ;
                        }
                        else if(theta < theta1)
                        {
                            theta2 = theta1;
                            theta1 = theta;
                            Hip3 = Hip2;
                            Hip2 = catalog[i].Id;
                            C = B;
                            B = NewXYZ;
                        }
                    }
                }

                //Внутренние векторы и величины
                Vec1 = B - A;
                Vec2 = C - A;

                v1 = Vec1.Length();
                v2 = Vec2.Length();

                phi = Math.Acos(Vector3.Dot(Vec1, Vec2) / (v1 * v2));

                if(theta1 > theta2)
                {
                    ang1 = theta2;
                    ang2 = theta1;
                    Hiparc2 = Hip3;
                    Hiparc3 = Hip2;
                }
                else
                {
                    ang1 = theta1;
                    ang2 = theta2;
                    Hiparc2 = Hip2;
                    Hiparc3 = Hip3;
                }

                feat.Add(new Triangles(Hip1, Hiparc2, Hiparc3, ang1, ang2, phi));
            }

            Console.WriteLine("Треугольники сформированы");
            return feat;
        }

        /// <summary>
        /// Аналог функции norm из MatLab
        /// </summary>
        private static double Vector3Norm(Vector3 v)
        {
            return Math.Sqrt(Math.Pow(v.X, 2) + Math.Pow(v.Y, 2) + Math.Pow(v.Z, 2));
        }

        private static int FoundListBoolTrue(List<int> index)
        {
            for(int i = 1; i < index.Count; i++)
                if(index[i] == 1)
                    return i;

            return -1;
        }

        public static List<Pattern> GetThreeStar_ID(List<Star> catalog, List<Triangles> featurelist, List<StarID> spotlist, double ecat)
        {
            List<Pattern> pattern=new List<Pattern>();

            pattern.Add(new Pattern(-1, -1, -1, -1, -1, -1));   // заплатка, чтоб всё было с единицы

            Vector3 A;
            Vector3 B;
            Vector3 C;

            Vector3 Vec1,Vec2;

            int spot1,spot2,spot3;
            int Spot2, Spot3;

            double theta, theta1,theta2;

            Vector3 New;

            double v1,v2;

            double phi;

            double ang1,ang2;

            for(int j = 1; j <= spotlist.Count; j++)
            {
                A = spotlist[j].XYZ;
                B = C = Vector3.Zero;

                spot1 = spotlist[j].Spot;
                spot2 = spot3 = 0;

                theta1 = theta2 = 360;

                for(int i = 1; i <= spotlist.Count; i++)
                {
                    New = spotlist[i].XYZ;
                    if(!New.Equals(A))
                    {
                        theta = Math.Acos(Vector3.Dot(A, New) / (Vector3Norm(A) * Vector3Norm(New)));

                        if(theta < theta2 && theta > theta1)
                        {
                            theta2 = theta;
                            spot3 = spotlist[i].Spot;
                            C = New;
                        }
                        else if(theta < theta1)
                        {
                            theta2 = theta1;
                            theta1 = theta;
                            spot3 = spot2;
                            spot2 = spotlist[i].Spot;
                            C = B;
                            B = New;
                        }
                    }
                }

                //ПОЛУЧИТЬ внутренний угол (фи)
                Vec1 = B - A;
                Vec2 = C - A;

                v1 = Vec1.Length();
                v2 = Vec2.Length();

                phi = Math.Acos(Vector3.Dot(Vec1, Vec2) / (v1 * v2));

                if(theta1 > theta2)
                {
                    ang1 = theta2;
                    ang2 = theta1;

                    Spot2 = spot3;
                    Spot3 = spot2;
                }
                else
                {
                    ang1 = theta1;
                    ang2 = theta2;

                    Spot2 = spot2;
                    Spot3 = spot3;
                }

                pattern.Add(new Pattern(spot1, Spot2, Spot3, ang1, ang2, phi));
            }

            // Поиск Список возможностей ПОЛУЧИТЬ совпадения

            List<Pattern> match=new List<Pattern>();
            match.Add(new Pattern(-1, -1, -1, -1, -1, -1));

            List<double>fAng1=new List<double>();
            List<double>fAng2=new List<double>();
            List<double>fAng3=new List<double>();

            for(int i = 0; i < featurelist.Count; i++)
            {
                fAng1.Add(featurelist[i].Theta1);
                fAng2.Add(featurelist[i].Theta2);
                fAng3.Add(featurelist[i].Phi);
            }

            for(int i = 1; i < pattern.Count; i++)
            {
                double high1=pattern[i].Theta1+ecat;
                double low1=pattern[i].Theta1-ecat;

                double high2=pattern[i].Theta2+ecat;
                double low2=pattern[i].Theta2-ecat;

                double high3=pattern[i].Phi+ecat;
                double low3=pattern[i].Phi-ecat;

                #region аналог булевских операций из матлаба с массивами

                //fAng1 = [featurelist.feat.theta1];
                //fAng2 = [featurelist.feat.theta2];
                //fAng3 = [featurelist.feat.phi];
                //for i = 1:N

                // high1 = pattern(i).theta1 + ecat;
                // low1 = pattern(i).theta1 - ecat;
                // high2 = pattern(i).theta2 + ecat;
                //low2 = pattern(i).theta2 - ecat;
                //high3 = pattern(i).phi + ecat;
                //low3 = pattern(i).phi - ecat;
                //ind1 = fAng1 <= high1;
                //ind2 = fAng1 >= low1;
                //ind3 = fAng2 <= high2;
                //ind4 = fAng2 >= low2;
                //ind5 = fAng3 <= high3;
                //ind6 = fAng3 >= low3;

                // по идее массивы должны быть булевские, но с int потом легче работать
                List<int>ind1=new List<int>();
                List<int>ind2=new List<int>();
                List<int>ind3=new List<int>();
                List<int>ind4=new List<int>();
                List<int>ind5=new List<int>();
                List<int>ind6=new List<int>();

                ind1.Add(0);
                ind2.Add(0);
                ind3.Add(0);
                ind4.Add(0);
                ind5.Add(0);
                ind6.Add(0);

                for(int ll = 1; ll < fAng1.Count; ll++)
                {
                    ind1.Add(fAng1[ll] <= high1 ? 1 : 0);
                    ind1.Add(fAng1[ll] <= low1 ? 1 : 0);
                    ind1.Add(fAng2[ll] <= high2 ? 1 : 0);
                    ind1.Add(fAng2[ll] <= low2 ? 1 : 0);
                    ind1.Add(fAng3[ll] <= high3 ? 1 : 0);
                    ind1.Add(fAng3[ll] <= low3 ? 1 : 0);
                }

                List<int> index=new List<int>();
                index.Add(0);

                for(int ll = 1; ll < ind1.Count; ll++)
                {
                    index.Add((ind1[ll] + ind2[ll] + ind3[ll] + ind4[ll] + ind5[ll] + ind5[ll]) == 0 ? 0 : 1);
                }

                #endregion аналог булевских операций из матлаба с массивами

                if(index.Sum() == 0)
                {
                    match.Add(new Pattern(-1, -1, -1, -1, -1, -1));
                }
                else
                {
                    int IndexTrue=FoundListBoolTrue(index);
                    match.Add(new Pattern(featurelist[IndexTrue].HipID1,
                                            featurelist[IndexTrue].HipID2,
                                            featurelist[IndexTrue].HipID3,
                                            featurelist[IndexTrue].Theta1,
                                            featurelist[IndexTrue].Theta2,
                                            featurelist[IndexTrue].Phi));
                }
            }

            //Спаривание мест для звезд (мини-голосование)

            List<StarID> starID=new List<StarID>();
            starID.Add(new StarID(-1, -1, -1, -1, -1, -1));

            List<int> ps1=new List<int>();
            List<int> ps2=new List<int>();
            List<int> ps3=new List<int>();

            List<int> ms1=new List<int>();
            List<int> ms2=new List<int>();
            List<int> ms3=new List<int>();

            ps1.Add(0);
            ps2.Add(0);
            ps3.Add(0);

            ms1.Add(0);
            ms2.Add(0);
            ms3.Add(0);

            for(int i = 1; i < pattern.Count; i++)
            {
                ps1.Add(pattern[i].Spot1);
                ps2.Add(pattern[i].Spot2);
                ps3.Add(pattern[i].Spot3);
            }

            for(int i = 1; i < match.Count; i++)
            {
                ms1.Add(match[i].Spot1);
                ms2.Add(match[i].Spot2);
                ms3.Add(match[i].Spot3);
            }

            for(int i = 1; i < spotlist.Count; i++)
            {
                List<int>hip1=new List<int>();
                List<int>hip2=new List<int>();
                List<int>hip3=new List<int>();
                List<int>hip=new List<int>();

                //hip1.Add(-1);
                //hip2.Add(-1);
                //hip3.Add(-1);

                for(int j = 1; j < ms1.Count; j++)
                {
                    if(ps1[j] == i)
                    {
                        hip1.Add(ms1[j]);
                    }

                    if(ps2[j] == i)
                    {
                        hip2.Add(ms2[j]);
                    }

                    if(ps3[j] == i)
                    {
                        hip3.Add(ms3[j]);
                    }
                }

                hip.AddRange(hip1);
                hip.AddRange(hip2);
                hip.AddRange(hip3);

                hip.RemoveAll(p => p == 0); // удаление всего, что равно 0
            }

            return pattern;
        }
    }
}