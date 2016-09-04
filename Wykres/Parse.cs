using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wykres
{
    class Parse
    {
        private string _str;
        public List<Package> list;
        //Wpublic List<Package> queOfPackage;
        public  delegate  void  onBufferFullDelegate(object sender);
        public  event onBufferFullDelegate onBufferFullEvent;
        

        public Parse()
        {
            list = new List<Package>();
            //queOfPackage = new List<Package>();
            //nowy komentarz
        }

        public async Task doParse(string str, params char[] ch)
        {
            var formatTokens = str.Split(ch);
            Task<List<Package>>[] t;
                        
            Task<List<Package>> first = Task.Run<List<Package>>(() =>parseByChar(formatTokens, ':', 0, formatTokens.Length / 8));
            Task<List<Package>> second = Task.Run<List<Package>>(() => parseByChar(formatTokens, ':', formatTokens.Length / 8, formatTokens.Length / 4));
            Task<List<Package>> third = Task.Run<List<Package>>(() =>  parseByChar(formatTokens, ':', formatTokens.Length / 4, 3/8*formatTokens.Length));
            Task<List<Package>> fourth = Task.Run<List<Package>>(() =>parseByChar(formatTokens, ':', 3 / 8 * formatTokens.Length, formatTokens.Length/2));
            Task<List<Package>> fifith = Task.Run<List<Package>>(() => parseByChar(formatTokens, ':', formatTokens.Length / 2, 5/8*formatTokens.Length));
            Task<List<Package>> sixth = Task.Run<List<Package>>(() => parseByChar(formatTokens, ':', 5 / 8 * formatTokens.Length, 6 / 8 * formatTokens.Length));
            Task<List<Package>> seventh = Task.Run<List<Package>>(() => parseByChar(formatTokens, ':',6 / 8 * formatTokens.Length, 7 / 8 * formatTokens.Length));
            Task<List<Package>> eight = Task.Run<List<Package>>(() => parseByChar(formatTokens, ':',7 / 8 * formatTokens.Length, formatTokens.Length));
            
            Task.WaitAll(first, second, third, fourth, fifith, sixth, seventh, eight);
            lock (list)
            {
                list.AddRange(first.Result);
                list.AddRange(second.Result);
                list.AddRange(third.Result);
                list.AddRange(fourth.Result);
                list.AddRange(fifith.Result);
                list.AddRange(sixth.Result);
                list.AddRange(seventh.Result);
                list.AddRange(eight.Result);
                if (list.Count > 1)
                {
                    onBufferFullEvent(list);
                }

            }  
            /*             
            await first;
            await second;
            await third;
            await fourth;
            await fifith;
            await sixth;
            await seventh;
            await eight;*/
        }

        private List<Package> parseByChar(string[] str, char ch, int begin, int end)
        {
            List<Package> tempList = new List<Package>();
                for (int i = begin; i < end; i++)
                {
                    lock (str)
                    {
                    str[i] = str[i].Replace("<", "");
                    str[i] = str[i].Replace(">", "");
                    str[i] = str[i].Replace("\n", "");
                    str[i] = str[i].Replace("\0", "");
                    var formatTokens = str[i].Split(ch);
                    if (formatTokens.Length == 5)
                    {

                        Task<float> first = Task.Run<float>(() => float.Parse(formatTokens[0], CultureInfo.CurrentCulture));
                        Task<float> second = Task.Run<float>(() => float.Parse(formatTokens[1], CultureInfo.CurrentCulture));
                        Task<float> third = Task.Run<float>(() => float.Parse(formatTokens[2], CultureInfo.CurrentCulture));
                        Task<float> fourth = Task.Run<float>(() => float.Parse(formatTokens[3], CultureInfo.CurrentCulture));
                        Task<float> fifth = Task.Run<float>(() => float.Parse(formatTokens[4], CultureInfo.CurrentCulture));
                        Task.WaitAll(first, second, third, fourth, fifth);
                        Package package = new Package(first.Result, second.Result, third.Result, fourth.Result, fifth.Result);
                                        
                        tempList.Add(package);
                       
                    }
                    }
                        
                }
            return tempList;
        }
    }
}

       


