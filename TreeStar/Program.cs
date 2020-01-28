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
            }

            return pattern;
        }
    }
}