﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using DataMining;
using DataMining.Distributions;

namespace TestApplication
{
    public class ExpediaReader
    {
        private int _count;
        private bool _loaded = false;

        private Dictionary<int, int>[] _symbols;

        private int date_time = 0;
        private int site_name = 1;
        private int posa_continent = 2;
        private int user_location_country = 3;
        private int user_location_region = 4;
        private int user_location_city = 5;
        private int orig_destination_distance = 6;
        private int user_id = 7;
        private int is_mobile = 8;
        private int is_package = 9;
        private int channel = 10;
        private int srch_ci = 11;
        private int srch_co = 12;
        private int srch_adults_cnt = 13;
        private int srch_children_cnt = 14;
        private int srch_rm_cnt = 15;
        private int srch_destination_id = 16;
        private int srch_destination_type_id = 17;
        private int hotel_continent = 18;
        private int hotel_country = 19;
        private int hotel_market = 20;
        private int is_booking = 21;
        private int cnt = 22;
        private int hotel_cluster = 23;
        private int[] _maximumValues = new int[24];
        private string _trainPath = @"C:\Working Projects\Kaggle\Expedia\train.csv";
        private string _testPath = @"C:\Working Projects\Kaggle\Expedia\test.csv";
        private int _numberOfLines = 0;
        private static ComparerLikelyhood _comparerLikelyhood = new ComparerLikelyhood();
        //private double[,][] _probabilities;
        //private double[] _classesValues;

        private IDistribution[,] _distribution;
        private IDistribution _classesProbablityDistribution;
        private NaiveBayesClassifier _classifier;                
        
        //private  List<int>[,] _nGramProbabilities;

        private void Init()
        {
            _maximumValues[0] = 12;
            _maximumValues[1] = 53;
            _maximumValues[2] = 4;
            _maximumValues[3] = 239;
            _maximumValues[4] = 1027;
            _maximumValues[5] = 56508;
            //_maximumValues[6] = 12408/100;
            _maximumValues[6] = 12408;
            _maximumValues[7] = 1198785;
            _maximumValues[8] = 1;
            _maximumValues[9] = 1;
            _maximumValues[10] = 10;
            _maximumValues[11] = 12;
            _maximumValues[12] = 12;
            _maximumValues[13] = 9;
            _maximumValues[14] = 9;
            _maximumValues[15] = 8;
            _maximumValues[16] = 65107;
            _maximumValues[17] = 9;
            _maximumValues[18] = 10;
            _maximumValues[19] = 269;
            _maximumValues[20] = 7;
            _maximumValues[21] = 212;
            _maximumValues[22] = 2117;
            _maximumValues[23] = 99;

            _numberOfLines = 37670293;
            //2147483647	
            //1198785



            //if (!_loaded)
            //{
            //    using (var textString = new StreamReader(@"C:\Research\Kaggle\Expedia\trainData\train.csv",
            //        System.Text.Encoding.ASCII, false, 100000000))
            //    {
            //        var line = textString.ReadLine();
            //        while (!textString.EndOfStream)
            //        {
            //            _numberOfLines++;
            //            line = textString.ReadLine();
            //            var columms = line.Split(",".ToCharArray());
            //            for (var index = 0; index < 24; index++)
            //            {
            //                var value = GetDataFromLine(columms,index);
            //                if (value.HasValue && value.Value > _maximumValues[index])
            //                {
            //                    _maximumValues[index] = value.Value;
            //                }

            //            }

            //            //var columms = line.Split(",".ToCharArray());
            //            //var dateTime = DateTime.Parse(columms[0]);
            //            //var siteName = int.Parse(columms[1]);
            //            //var posa_continent = int.Parse(columms[2]);
            //            //var user_location_country = int.Parse(columms[3]);
            //            //var user_location_region = int.Parse(columms[4]);
            //            //var user_location_city = int.Parse(columms[5]);
            //            //var orig_destination_distance = int.Parse(columms[6]);
            //            //var user_id = int.Parse(columms[7]);
            //            //var is_mobile = int.Parse(columms[8]);
            //            //var is_package = int.Parse(columms[9]);
            //            //var channel = int.Parse(columms[10]);
            //            //var srch_ci = int.Parse(columms[11]);
            //            //var srch_co = int.Parse(columms[12]);
            //            //var srch_adults_cnt = int.Parse(columms[13]);
            //            //var srch_children_cnt = int.Parse(columms[14]);
            //            //var srch_rm_cnt = int.Parse(columms[15]);
            //            //var srch_destination_id = int.Parse(columms[16]);
            //            //var srch_destination_type_id = int.Parse(columms[17]);
            //            //var hotel_continent = int.Parse(columms[18]);
            //            //var hotel_country = int.Parse(columms[19]);
            //            //var hotel_market = int.Parse(columms[20]);
            //            //var is_booking = int.Parse(columms[21]);
            //            //var cnt = int.Parse(columms[22]);
            //            //var classId = int.Parse(columms[23]);
            //            //if (!hashSet.Contains(classId))
            //            //{
            //            //    hashSet.Add(classId);
            //            //}
            //        }
            //    }

            //    _loaded = true;
            //}
            _loaded = true;
            Classes = _maximumValues[23] + 1;
        }

        [Serializable]
        public class Context
        {

            public Dictionary<int, int>[] Symbols { get; set; }

            public IDistribution[,] Distribution { get; set; }
            public IDistribution ClassesProbablityDistribution { get; set; }

        }

        private void LoadProbabilities(List<int[]> nGrams = null)
        {
            if (nGrams == null)
            {
                nGrams = new List<int[]>();
            }
            var totalColumns = 23 + nGrams.Count();

            var probabilities = new double[Classes, totalColumns][];
            var nGramProbabilities = new List<int>[Classes, nGrams.Count()];
            var classesValues = new double[Classes];
            _symbols = new Dictionary<int, int>[nGrams.Count];

            _distribution = new IDistribution[Classes, totalColumns];

            for (int index = 0; index < nGrams.Count; index++)
            {
                _symbols[index] = new Dictionary<int, int>();
                for (int jindex = 0; jindex < Classes; jindex++)
                {
                    nGramProbabilities[jindex, index] = new List<int>();
                }

            }
            for (int index = 0; index < Classes; index++)
            {
                for (int jIndex = 0; jIndex < 23; jIndex++)
                {
                    probabilities[index, jIndex] = new double[_maximumValues[jIndex] + 1];
                }
            }


            DateTime dt = DateTime.Now;
            //var buffer = 100000000;
            var buffer = 5000000;
            var dataBuffer = new int?[23];

            using (
                var textString = new StreamReader(_trainPath,
                    Encoding.ASCII, false, buffer))
            {
                var line = textString.ReadLine();
                while (!textString.EndOfStream)
                {

                    line = textString.ReadLine();
                    var columms = line.Split(',');
                    var classVal = GetDataFromLine(columms, 23).Value;
                    classesValues[classVal]++;
                    for (var index = 0; index < 23; index++)
                    {
                        dataBuffer[index] = GetDataFromLine(columms, index);
                        if (dataBuffer[index].HasValue)
                        {
                            var value = dataBuffer[index].Value;
                            probabilities[classVal, index][value]++;
                        }
                    }
                    var newColumndIndex = 0;

                    foreach (var nGram in nGrams)
                    {
                        var code = GetDataFromLine(dataBuffer, nGram);

                        var dict = _symbols[newColumndIndex];

                        int value;
                        if (!dict.TryGetValue(code, out value))
                        {
                            dict.Add(code, dict.Count);

                            nGramProbabilities[classVal, newColumndIndex].Add(dict.Count - 1);
                        }
                        else
                        {
                            nGramProbabilities[classVal, newColumndIndex].Add(value);
                        }


                        newColumndIndex ++;
                    }

                }
            }

            var ts = DateTime.Now.Subtract(dt);
            Console.WriteLine(ts.TotalMinutes);
            GC.Collect();
            for (int index = 0; index < nGramProbabilities.GetLength(0); index++)
            {
                for (int jindex = 0; jindex < nGramProbabilities.GetLength(1); jindex++)
                {
                    var dict = _symbols[jindex];
                    probabilities[index, 23 + jindex] = new double[dict.Count + 1];
                    foreach (var value in nGramProbabilities[index, jindex])
                    {
                        probabilities[index, 23 + jindex][value]++;
                    }
                }
            }

            for (int index = 0; index < probabilities.GetLength(0); index++)
            {
                for (int jindex = 0; jindex < probabilities.GetLength(1); jindex++)
                {
                    _distribution[index, jindex] = new CategoricalDistribution(probabilities[index, jindex]);                   
                }
            }

            _classesProbablityDistribution = new CategoricalDistribution(classesValues);

            _classifier = new NaiveBayesClassifier(_distribution, _classesProbablityDistribution);
            Serialize();
        }



        public void Serialize()
        {
            var context = new Context() { Symbols = _symbols, ClassesProbablityDistribution = _classesProbablityDistribution, Distribution = _distribution };
            FileStream fs = new FileStream("DataFile.dat", FileMode.Create);

            // Construct a BinaryFormatter and use it to serialize the data to the stream.
            BinaryFormatter formatter = new BinaryFormatter();
            try
            {
                formatter.Serialize(fs, context);
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }

        public void Derialize()
        {

            // Open the file containing the data that you want to deserialize.
            FileStream fs = new FileStream("DataFile.dat", FileMode.Open);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();

                // Deserialize the hashtable from the file and 
                // assign the reference to the local variable.
                var context = (Context) formatter.Deserialize(fs);
                this._distribution = context.Distribution;
                this._classesProbablityDistribution = context.ClassesProbablityDistribution;
                this._symbols = context.Symbols;
            }
            catch (SerializationException e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            finally
            {
                fs.Close();
            }
        }


        public ExpediaReader()
        {
            Init();

            var ngrams = new List<int[]>();
            //ngrams.Add(new[] {3, 16, 18, 19});
            //ngrams.Add(new[] {3, 16, 18, 19, 20});
            //ngrams.Add(new[] {4, 16, 18, 19});
            //ngrams.Add(new[] {5, 16, 18, 19});
            //ngrams.Add(new[] {7, 16});
            //ngrams.Add(new[] {7, 20});

            ngrams.Add(new[] { user_location_city, orig_destination_distance });
            ngrams.Add(new[] { user_id, user_location_city, srch_destination_id, hotel_country, hotel_market });
            ngrams.Add(new[] { user_id, srch_destination_id, hotel_country, hotel_market });
            ngrams.Add(new[] { srch_destination_id, hotel_country, hotel_market, is_package });
            ngrams.Add(new[] { hotel_market });
            
            LoadProbabilities(ngrams);
            Estimate();

        }

        public void Estimate()
        {
            var ngrams = new List<int[]>();
            //ngrams.Add(new[] {3, 16, 18, 19});
            //ngrams.Add(new[] {3, 16, 18, 19, 20});
            //ngrams.Add(new[] {4, 16, 18, 19});
            //ngrams.Add(new[] {5, 16, 18, 19});
            //ngrams.Add(new[] {7, 16});
            //ngrams.Add(new[] {7, 20});

            ngrams.Add(new[] { user_location_city, orig_destination_distance });
            ngrams.Add(new[] { user_id, user_location_city, srch_destination_id, hotel_country, hotel_market });
            ngrams.Add(new[] { user_id, srch_destination_id, hotel_country, hotel_market });
            ngrams.Add(new[] { srch_destination_id, hotel_country, hotel_market, is_package });
            ngrams.Add(new[] { hotel_market });
            //var dataSamples = GetDataSamples(Enumerable.Range(0, 20), ngrams);
            var dataSamples = GetDataSamples(Enumerable.Empty<int>(), ngrams);
            var index = 0;


            var collectionPartitioner = Partitioner.Create(0, dataSamples.Count);

            Parallel.ForEach(collectionPartitioner, (range, loopState) =>
            {
                ClassLikelyhood[] resultData = new ClassLikelyhood[2*Classes];
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    GetLikelyhood(dataSamples[i], resultData);
                    dataSamples[i].Tag = resultData[0].ClassId + " " + resultData[1].ClassId + " " +
                                         resultData[2].ClassId + " " +
                                         resultData[3].ClassId + " " + resultData[4].ClassId;
                }
            });


            using (var sw = new StreamWriter(string.Format("Submission_{0}", DateTime.Now.ToString("dd-MM-yy hh-mm"))))
            {
                sw.WriteLine("id,hotel_cluster");
                foreach (var sample in dataSamples)
                {
                    var data = index + "," + sample.Tag;
                    sw.WriteLine(data);
                    index ++;
                }
            }

        }
        

        private int? GetDataFromLine(string[] line, int columnIndex, bool isTest = false)
        {
            if (columnIndex <= 23)
            {
                var data = !isTest ? line[columnIndex] : line[columnIndex + 1];

                if (columnIndex == date_time || columnIndex == srch_ci || columnIndex == srch_ci)
                {
                    DateTime date;
                    if (DateTime.TryParse(data, out date))
                    {
                        return date.Month;
                    }
                }
                else if (columnIndex == orig_destination_distance)
                {
                    double value;
                    if (Double.TryParse(data, out value))
                    {
                        return Convert.ToInt32(value);
                    }
                }
                else
                {
                    int value;
                    if (int.TryParse(data, out value))
                    {
                        return value;
                    }
                }
            }
            return null;
        }

        private int GetDataFromLine(string[] line, int[] columnIndexex, bool isTest = false)
        {
            var maxValue = 2000000;
            var codeToRet = 0;
            foreach (var column in columnIndexex)
            {
                var ret = GetDataFromLine(line, column, isTest);

                if (ret.HasValue)
                {
                    codeToRet += maxValue + ret.Value;
                }
                maxValue = maxValue << 1;
            }
            return codeToRet;
        }

        private int GetDataFromLine(int?[] values, int[] columnIndexex, bool isTest = false)
        {
            var maxValue = 2000000;
            var codeToRet = 0;
            foreach (var column in columnIndexex)
            {
                var ret = values[column];

                if (ret.HasValue)
                {
                    codeToRet += maxValue + ret.Value;
                }
                maxValue = maxValue << 1;
            }
            return codeToRet;
        }

        public void GetLikelyhood(DataSample sample, ClassLikelyhood[] result)
        {
            var matched = new int[Classes];
            var currentIindex = 0;


            foreach (var dataPoint in sample.DataPoints)
            {
                var itemsFound = 0;
                for (int index = 0; index < Classes; index++)
                {
                    if (matched[index] != 0)
                    {
                        continue;
                    }
                    
                    var value = Convert.ToDouble(dataPoint.Value);
                    var prob = _distribution[index, dataPoint.ColumnId].GetProbability(value);

                    if (prob > Double.Epsilon)
                    {
                        result[currentIindex + itemsFound].ClassId = index;
                        result[currentIindex + itemsFound].Value = prob;
                        itemsFound++;
                        matched[index]++;
                    }
                }
                if (itemsFound > 0)
                {
                    Array.Sort(result, currentIindex, itemsFound, _comparerLikelyhood);
                    currentIindex = currentIindex + itemsFound;
                }

                if (currentIindex >= 5)
                {
                    return;
                }
            }

            if (currentIindex < 5)
            {
                for (int index = 0; index < Classes; index++)
                {
                    if (matched[index] != 0)
                    {
                        continue;
                    }


                    var prob = _classesProbablityDistribution.GetProbability(index);

                    result[currentIindex + index].ClassId = index;
                    result[currentIindex + index].Value = prob;                                        

                }

                Array.Sort(result, currentIindex, Classes, _comparerLikelyhood);
            }

            return;
        }

        public int Classes { get; set; }
       

        public IList<DataSample> GetDataSamples(IEnumerable<int> fields, List<int[]> nGrams = null)
        {
            var listSamples = new List<DataSample>();
            using (
                var textString = new StreamReader(_testPath,
                    Encoding.ASCII, false, 100000000))
            {
                var line = textString.ReadLine();
                while (!textString.EndOfStream)
                {
                    line = textString.ReadLine();
                    var columns = line.Split(",".ToCharArray());
                    var dataSample = new DataSample();
                    var dataPoints = new List<DataPoint>();
                    foreach (var field in fields)
                    {
                        var data = GetDataFromLine(columns, field, true);
                        if (data.HasValue)
                        {
                            dataPoints.Add(new DataPoint() { ColumnId = field, Value = data.Value });
                        }                        
                    }
                    var columnIndex = 0;
                    foreach (var field in nGrams)
                    {
                        var data = GetDataFromLine(columns, field, true);
                        var dict = _symbols[columnIndex];
                        if (dict.ContainsKey(data))
                        {
                            dataPoints.Add(new DataPoint() { ColumnId = columnIndex + 23, Value = dict[data] });
                        }                       
                        columnIndex++;
                    }
                    dataSample.DataPoints = dataPoints.ToArray();
                    listSamples.Add(dataSample);

                }
            }

            return listSamples;
        }

        private class ComparerLikelyhood : IComparer<ClassLikelyhood>
        {
            public int Compare(ClassLikelyhood x, ClassLikelyhood y)
            {
                if (x.Value > y.Value)
                {
                    return -1;
                }
                else if (x.Value < y.Value)
                {
                    return 1;
                }
                return 0;
            }
        }

        public struct ClassLikelyhood
        {
            public int ClassId { get; set; }
            public double Value { get; set; }
        }

    }
}


