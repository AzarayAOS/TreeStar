﻿using System;
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
        public class SpotList
        {
            public SpotList(int spot, Vector3 xYZ)
            {
                Spot = spot;
                XYZ = xYZ;
            }

            public int Spot { get; set; }
            public Vector3 XYZ { get; set; }
        }

        public class Matrix
        {
            public Matrix(int voites, int spot, double hipID, int trueH, Vector3 xYZ)
            {
                Voites = voites;
                Spot = spot;
                HipID = hipID;
                TrueH = trueH;
                XYZ = xYZ;
            }

            public int Voites { get; set; }
            public int Spot { get; set; }
            public double HipID { get; set; }
            public int TrueH { get; set; }
            public Vector3 XYZ { get; set; }
        }

        public class Stats
        {
            public Stats()
            {
                TrueID =
                    FalseID =
                    NeutralID =
                    Quality =
                    NoSol =
                    EmptySol = 0;

                RCvalue =
                    PercFalse = 0;
            }

            public int TrueID { get; set; }
            public int FalseID { get; set; }
            public int NeutralID { get; set; }
            public double RCvalue { get; set; }
            public int Quality { get; set; }
            public double PercFalse { get; set; }
            public int NoSol { get; set; }
            public int EmptySol { get; set; }
        }

        public class Sky
        {
            public Sky(int hipID, Vector3 xYZ)
            {
                HipID = hipID;
                XYZ = xYZ;
            }

            public Sky(int hipID, double x, double y, double z)
            {
                HipID = hipID;
                XYZ = new Vector3(
                    Convert.ToSingle(x),
                    Convert.ToSingle(y),
                    Convert.ToSingle(z));
            }

            public int HipID { get; set; }
            public Vector3 XYZ { get; set; }
        }

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
            #region Входные данные из вне программы

            //string FileCatalog="valuesID.csv";      // путь к файлу каталога
            string FileCatalog="asu_hipparcos_catalog_ra_dec_deg_wo_header.txt";
            string FileScreen="star.csv";       // путь к файлу с RA и DEC снимка

            string FileTriadStarMap="TriadStar_M7_F20.csv";

            #endregion Входные данные из вне программы

            int MRSS=5;                 // Минимально необходимое количество звезд для решения

            double Mg=7;
            Stats stats=new Stats();
            Char separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator[0];
            double eps=Math.Pow(10,-3);     // погрешность
            double Ra=0;
            double Dec=0;

            double FOV=20;
            DateTime dateTime=DateTime.Now;
            Console.Write("GetSpotList =>   \t\t");
            List<SpotList> spotLists=GetSpotList(FileScreen,separator);          // каталог звёзд, положение которых надо найти
            Console.WriteLine("Время выполнения: " + DateTime.Now.Subtract(dateTime).ToString());

            Console.Write("CreateCatalogTriad =>   \t");
            List<Star>CatalogStar= CreateCatalogTriad(Mg, FileCatalog, separator);          // загружаем каталог звёзд с определёнными зв.величинами
            Console.WriteLine("Время выполнения: " + DateTime.Now.Subtract(dateTime).ToString());

            Console.Write("ConvertToSky =>   \t\t");
            List<Sky> sky=ConvertToSky(CatalogStar);                // фильтрация каталога от ненужных данных
            Console.WriteLine("Время выполнения: " + DateTime.Now.Subtract(dateTime).ToString());

            Console.Write("Triad_Feature_Extract =>   \t");
            //List<Triangles>featurelist2=Triad_Feature_Extract(Mg,FOV,CatalogStar);          // создание списка возможных вариаций

            List<Triangles>featurelist2=ReadTriadStar(FileTriadStarMap,separator);          // загрузка списка возможных вариаций

            Console.WriteLine("Время выполнения: " + DateTime.Now.Subtract(dateTime).ToString());

            Console.Write("GetThreeStar_ID =>   \t\t");
            List<StarID> starIDs=GetThreeStar_ID(CatalogStar,featurelist2,spotLists,eps);   // сравниваем
            Console.WriteLine("Время выполнения: " + DateTime.Now.Subtract(dateTime).ToString());

            Console.Write("IDAccuracy =>   \t\t");
            List<Matrix> matrix=IDAccuracy(starIDs,sky,MRSS, ref stats);                         // идентифицируем звёзды
            Console.WriteLine("Время выполнения: " + DateTime.Now.Subtract(dateTime).ToString());
            Console.WriteLine();

            matrix = matrix.OrderByDescending(x => x.Voites).ToList();
            Console.WriteLine();
            PrintMatrix(matrix);

            CalcalateRaDec(matrix, CatalogStar, ref Ra, ref Dec);

            Console.WriteLine("\n\nRa:  {0,5}\n" +
                              "Dec: {1,5}", Ra, Dec);

            Console.WriteLine();
            PrintStatisica(stats);

            starIDs = starIDs.OrderByDescending(x => x.Votes).ToList();
            Console.WriteLine();
            PrintStarID(starIDs);
            Console.ReadKey();
        }

        public static List<Triangles> ReadTriadStar(string FileNamem, Char separator)
        {
            List<Triangles> feat=new List<Triangles>();
            using(StreamReader sr = new StreamReader(FileNamem, System.Text.Encoding.Default))
            {
                string line;
                line = sr.ReadLine();
                char[] spl={',','[',']','"'};

                while((line = sr.ReadLine()) != null)
                {
                    string[] ch = line.Split(spl);

                    int HipID1=Convert.ToInt32(ch[1]);
                    int HipID2=Convert.ToInt32(ch[2]);
                    int HipID3=Convert.ToInt32(ch[3]);

                    double theta1=Convert.ToDouble(ch[4].Replace('.', separator));
                    double theta2=Convert.ToDouble(ch[5].Replace('.', separator));

                    double phi=Convert.ToDouble(ch[6].Replace('.', separator));

                    feat.Add(new Triangles(HipID1, HipID2, HipID3, theta1, theta2, phi));
                }
            }

            return feat;
        }

        public static void PrintStarID(List<StarID> starIDs)
        {
            Console.WriteLine("|{0,4}|{1,5}|{2,5}|{3,27}|", "Spot", "Votes", "HipID", "XYZ");
            for(int i = 0; i < starIDs.Count; i++)
                Console.WriteLine("|" +
                    "{0,4}|" +
                    "{1,5}|" +
                    "{2,5}|" +
                    "{3,27}|",
                   starIDs[i].Spot,
                   starIDs[i].Votes,
                   starIDs[i].HipID,
                   starIDs[i].XYZ.ToString());
        }

        /// <summary>
        /// Из всех найденных звёзд вычисляется среднее положение каждой звезды, учитывая голосование
        /// </summary>
        public static void CalcalateRaDec(List<Matrix> matrices, List<Star> catalog, ref double Ra, ref double Dec)
        {
            int n=0;

            for(int i = 0; i < matrices.Count; i++)

                if(matrices[i].Voites != 0)
                {
                    n++;

                    //Ra += catalog.Where(x => x.Id == matrices[i].TrueH).ToList()[0].Ra;
                    //Dec += catalog.Where(x => x.Id == matrices[i].TrueH).ToList()[0].Dec;

                    Ra += matrices[i].Voites * catalog.Where(x => x.Id == matrices[i].TrueH).ToList()[0].Ra;
                    Dec += matrices[i].Voites * catalog.Where(x => x.Id == matrices[i].TrueH).ToList()[0].Dec;
                }

            Ra /= n;
            Dec /= n;
        }

        /// <summary>
        /// Вывод в консоль статистики <paramref name="stats" />
        /// </summary>
        public static void PrintStatisica(Stats stats)
        {
            Console.WriteLine("{0,9} => {1,4};", "EmptySol", stats.EmptySol);
            Console.WriteLine("{0,9} => {1,4};", "FalseID", stats.FalseID);
            Console.WriteLine("{0,9} => {1,4};", "NeutralID", stats.NeutralID);
            Console.WriteLine("{0,9} => {1,4};", "NoSol", stats.NoSol);
            Console.WriteLine("{0,9} => {1,4};", "PercFalse", stats.PercFalse);
            Console.WriteLine("{0,9} => {1,4};", "Quality", stats.Quality);
            Console.WriteLine("{0,9} => {1,4};", "RCvalue", stats.RCvalue);
            Console.WriteLine("{0,9} => {1,4};", "TrueID", stats.TrueID);
        }

        /// <summary>
        /// Выводит в консоль в виде таблицы данные из <paramref name="matrix" />
        /// </summary>
        public static void PrintMatrix(List<Matrix> matrix)
        {
            Console.WriteLine("|{0,5}|{1,4}|{2,5}|{3,7}|{4,27}|", "Votes", "Spot", "HipID", "TrueH", "XYZ");
            for(int i = 0; i < matrix.Count; i++)
                Console.WriteLine("|" +
                    "{0,5}|" +
                    "{1,4}|" +
                    "{2,5}|" +
                    "{3,7}|" +
                    "{4,27}|",
                    matrix[i].Voites,
                    matrix[i].Spot,
                    matrix[i].HipID,
                    matrix[i].TrueH,
                    matrix[i].XYZ.ToString());
        }

        /// <summary>
        /// Отсекает от каталога ненужные данные
        /// </summary>
        public static List<Sky> ConvertToSky(List<Star> catalog)
        {
            List<Sky> skies=new List<Sky>();

            for(int i = 0; i < catalog.Count; i++)
                skies.Add(new Sky(catalog[i].Id, catalog[i].GetXYZ()));

            return skies;
        }

        /// <summary>
        /// Чтение каталога <paramref name="datacatalog" /> и занесение всех звёзд с з. величиной
        /// равной и меньше <paramref name="Mg" />
        /// </summary>
        public static List<Star> CreateCatalogTriad(double Mg, string datacatalog, Char separator)
        {
            List<Star> CatalogStar = new List<Star>();
            //CatalogStar.Add(new Star(-1, -1, -1, -1));  // заглушка, чтоб всё было с единицы

            using(StreamReader sr = new StreamReader(datacatalog, System.Text.Encoding.Default))
            {
                string line;
                line = sr.ReadLine();
                //int i = 0;
                while((line = sr.ReadLine()) != null)
                {
                    string[] ch = line.Split(";");
                    if(ch[0] != "" & ch[1] != "" & ch[2] != "" & ch[3] != "")
                    {
                        int id = Convert.ToInt32(ch[0]);
                        float ra = Convert.ToSingle(ch[2].Replace('.', separator));
                        float dec = Convert.ToSingle(ch[3].Replace('.', separator));
                        float mgt = Convert.ToSingle(ch[1].Replace('.', separator));

                        if(mgt <= Mg)
                            CatalogStar.Add((new Star(id, ra, dec, mgt)));
                    }
                }
            }

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
            //feat.Add(new Triangles(-1, -1, -1, -1, -1, -1));    // заглушка, чтоб начиналос с 1го индекса

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
            int tempindex=0;

            for(int j = 0; j < catalog.Count; j++)
            {
                A = catalog[j].GetXYZ();    // Желаемый вектор для сравнения
                B = C = Vector3.Zero;

                Hip1 = catalog[j].Id;
                Hip2 = Hip3 = -1;

                theta1 = theta2 = 360;

                for(int i = 0; i < catalog.Count; i++)
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
                tempindex++;
                feat.Add(new Triangles(Hip1, Hiparc2, Hiparc3, ang1, ang2, phi));
            }

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

        /// <summary>
        /// 3-звездочный алгоритм идентификации звезды триады-Лейбе Получает точечные произведения и
        /// внутренние углы двух ближайших звезд к конкретной звезды.Это ограничено только двумя
        /// ближайшими звездами рядом с рассматриваемой звездой.
        /// </summary>
        public static List<StarID> GetThreeStar_ID(List<Star> catalog, List<Triangles> featurelist, List<SpotList> spotlist, double ecat)
        {
            List<Pattern> pattern=new List<Pattern>();

            //pattern.Add(new Pattern(-1, -1, -1, -1, -1, -1));   // заплатка, чтоб всё было с единицы

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

            for(int j = 0; j < spotlist.Count; j++)
            {
                A = spotlist[j].XYZ;
                B = C = Vector3.Zero;

                spot1 = spotlist[j].Spot;
                spot2 = spot3 = 0;

                theta1 = theta2 = 360;

                for(int i = 0; i < spotlist.Count; i++)
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
            //match.Add(new Pattern(-1, -1, -1, -1, -1, -1));

            List<double>fAng1=new List<double>();
            List<double>fAng2=new List<double>();
            List<double>fAng3=new List<double>();

            for(int i = 0; i < featurelist.Count; i++)
            {
                fAng1.Add(featurelist[i].Theta1);
                fAng2.Add(featurelist[i].Theta2);
                fAng3.Add(featurelist[i].Phi);
            }

            for(int i = 0; i < pattern.Count; i++)
            {
                double high1=pattern[i].Theta1+ecat;
                double low1=pattern[i].Theta1-ecat;

                double high2=pattern[i].Theta2+ecat;
                double low2=pattern[i].Theta2-ecat;

                double high3=pattern[i].Phi+ecat;
                double low3=pattern[i].Phi-ecat;

                #region аналог булевских операций из матлаба с массивами

                // по идее массивы должны быть булевские, но с int потом легче работать
                List<int>ind1=new List<int>();
                List<int>ind2=new List<int>();
                List<int>ind3=new List<int>();
                List<int>ind4=new List<int>();
                List<int>ind5=new List<int>();
                List<int>ind6=new List<int>();

                for(int ll = 0; ll < fAng1.Count; ll++)
                {
                    ind1.Add(fAng1[ll] <= high1 ? 1 : 0);
                    ind2.Add(fAng1[ll] >= low1 ? 1 : 0);
                    ind3.Add(fAng2[ll] <= high2 ? 1 : 0);
                    ind4.Add(fAng2[ll] >= low2 ? 1 : 0);
                    ind5.Add(fAng3[ll] <= high3 ? 1 : 0);
                    ind6.Add(fAng3[ll] >= low3 ? 1 : 0);
                }

                List<int> index=new List<int>();
                //index.Add(0);

                for(int ll = 0; ll < ind1.Count; ll++)
                {
                    index.Add((ind1[ll] + ind2[ll] + ind3[ll] + ind4[ll] + ind5[ll] + ind5[ll]) == 6 ? 1 : 0);
                }

                #endregion аналог булевских операций из матлаба с массивами

                if(index.Sum() == 0)
                {
                    match.Add(new Pattern(0, 0, 0, 0, 0, 0));
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
            //starID.Add(new StarID(-1, -1, -1, -1, -1, -1));

            List<int> ps1=new List<int>();
            List<int> ps2=new List<int>();
            List<int> ps3=new List<int>();

            List<int> ms1=new List<int>();
            List<int> ms2=new List<int>();
            List<int> ms3=new List<int>();

            for(int i = 0; i < pattern.Count; i++)
            {
                ps1.Add(pattern[i].Spot1);
                ps2.Add(pattern[i].Spot2);
                ps3.Add(pattern[i].Spot3);
            }

            for(int i = 0; i < match.Count; i++)
            {
                ms1.Add(match[i].Spot1);
                ms2.Add(match[i].Spot2);
                ms3.Add(match[i].Spot3);
            }

            for(int i = 0; i < spotlist.Count; i++)
            {
                List<int>hip1=new List<int>();
                List<int>hip2=new List<int>();
                List<int>hip3=new List<int>();
                List<int>hip=new List<int>();

                for(int j = 0; j < ms1.Count; j++)
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

                List<int>  uhip = hip.AsQueryable().Distinct().ToList();    // удаляем все повторения, делая лист с уникальными значеними
                List<int> votes=new List<int>();

                int vote;
                int index;

                if(uhip.Count != 0)
                {
                    for(int j = 0; j < uhip.Count; j++)
                        votes.Add(hip.Where(x => x == uhip[j]).Count());

                    vote = votes.Max();
                    index = votes.IndexOf(vote);

                    starID.Add(new StarID(votes.Max(), i, votes.IndexOf(vote), spotlist[i].XYZ));
                }
                else
                {
                    vote = index = 0;
                    starID.Add(new StarID(0, i, 0, spotlist[i].XYZ));
                }

                //starID.Add(new StarID(vote, i, uhip[index], spotlist[i].XYZ));
            }

            return starID;
        }

        /// <summary>
        /// Сопоставление найденных звёзд с каталогом и идентификация со статистикой
        /// </summary>
        public static List<Matrix> IDAccuracy(List<StarID> starID, List<Sky> sky, int MRSS, ref Stats stats)
        {
            List<int>votes=new List<int>();
            List<double>Hip=new List<double>();

            for(int i = 0; i < starID.Count; i++)
            {
                votes.Add(starID[i].Votes);
                Hip.Add(starID[i].HipID);
            }

            if(votes.Where(x => x > 0).ToList().Sum() == 0)
            {
                stats.TrueID = 0;
                stats.FalseID = 0;
                stats.NeutralID = 0;
                stats.PercFalse = 0;
                stats.NoSol = 0;
                stats.EmptySol = 100;
            }
            else if(Hip.Sum() != 0)
            {
                for(int i = 0; i < starID.Count; i++)
                {
                    if(starID[i].HipID == sky[i].HipID)
                        stats.TrueID++;
                    else
                    {
                        if(starID[i].Votes > 0)
                            stats.FalseID++;
                        else if(starID[i].Votes <= 0)
                            stats.NeutralID++;
                    }

                    if(i == sky.Count - 1)
                    {
                        if(stats.TrueID < MRSS || stats.FalseID > 0)
                            stats.NoSol = 100;
                        else
                            stats.NoSol = 0;
                    }
                }

                double diveder=Hip.Where(x=>x!=0).Sum();
                stats.PercFalse = stats.FalseID / diveder * 100;
            }
            else
            {
                stats.TrueID = 0;
                stats.FalseID = 0;
                stats.NeutralID = 0;
                stats.PercFalse = 0;
                stats.NoSol = 0;
                stats.EmptySol = 100;
            }
            // Надежность / Доверие и Качество голосов сумма голосов / максимальное количество
            // голосов / n истинных звезд = Надежность / Доверие

            List<int>VoiAbs=votes.Take(sky.Count).ToList();
            for(int ll = 0; ll < VoiAbs.Count; ll++)
                VoiAbs[ll] = Math.Abs(VoiAbs[ll]);

            int maxabs=VoiAbs.Sum();

            if(maxabs == 0)
                stats.RCvalue = -1;
            else
                stats.RCvalue = votes.Take(sky.Count).ToList().Sum() / (maxabs * sky.Count);

            stats.Quality = votes.Sum();

            List<Matrix> matrix=new List<Matrix>();

            for(int i = 0; i < starID.Count; i++)
            {
                matrix.Add(new Matrix(starID[i].Votes, starID[i].Spot, starID[i].HipID, sky[i].HipID, starID[i].XYZ));
                //matrix.Add(new Matrix(starID[i].Votes, starID[i].Spot, starID[i].HipID, 0, starID[i].XYZ));
            }

            return matrix;
        }

        /// <summary>
        /// Чтение данных с файла <paramref name="filedata" /> со зёздами с камеры
        /// </summary>
        public static List<SpotList> GetSpotList(string filedata, Char separator)
        {
            List<SpotList> spotList = new List<SpotList>();
            //CatalogStar.Add(new Star(-1, -1, -1, -1));  // заглушка, чтоб всё было с единицы

            using(StreamReader sr = new StreamReader(filedata, System.Text.Encoding.Default))
            {
                int index=0;
                string line;
                line = sr.ReadLine();
                //int i = 0;
                while((line = sr.ReadLine()) != null)
                {
                    string[] ch = line.Split(";");

                    index++;

                    float x = Convert.ToSingle(ch[0].Replace('.', separator));
                    float y = Convert.ToSingle(ch[1].Replace('.', separator));
                    float z = Convert.ToSingle(ch[2].Replace('.', separator));

                    spotList.Add(new SpotList(index, new Vector3(x, y, z)));
                }
            }

            return spotList;
        }
    }
}