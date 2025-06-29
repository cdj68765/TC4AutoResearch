using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace AutoResearch
{
    internal class Program
    {
        private class HexTree
        {
            public string HexString;
            public Hex Hex;
        }

        private class HexEntry
        {
            public Aspect aspect { get; set; }
            public int type { get; set; }
        }

        public class Aspect
        {
            public string tag { get; set; }
            public Aspect[] components { get; set; }
            public int color { get; set; }
            public string chatcolor { get; set; }
            public int blend { get; set; }
        }

        private class ResearchNoteData
        {
            public string key { get; set; }
            public int color { get; set; }
            public Dictionary<string, HexEntry> hexEntries { get; set; } = new();
            public Dictionary<string, Hex> hexes { get; set; } = new();
            public int copies { get; set; }
        }

        public class ResearchRet
        {
            public string[][] Path { get; set; }
            public string[][] Aspect { get; set; }
        }

        //private static Dictionary<string, int> PlayaspectList = new Dictionary<string, int>();
        //private static Dictionary<string, (int, int, string)> NoteHex = new Dictionary<string, (int, int, string)>();

        private static Dictionary<string, List<string>> AspectMap = new();
        private static ResearchNoteData Note;

        //private static List<Aspect> AllAspect = new();
        private static Dictionary<string, Aspect> AllAspect = new();

        private static Dictionary<string, int> UserAspect = new();
        //private static string[] Notes;

        public static void Solves2(string[] Notes)
        {
            //if (args.Length != 0)
            //    File.WriteAllText("Note.txt", args[0]);
            //if (File.Exists("Note.txt"))
            //    Notes = File.ReadAllText($@"Note.txt").Split('^');
            //else if (File.Exists($@"..\..\..\..\Note.txt"))
            //    Notes = File.ReadAllText($@"..\..\..\..\Note.txt").Split('^');
            //if (args.Length == 1)
            //    Notes = args[0].Split('^');
            if (Notes.Length == 3)
            {
                foreach (var item in Notes[1].Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var TempItem = item.Split(':');
                    if (TempItem.Length == 1)
                    {
                        AllAspect.Add(TempItem[0], new Aspect() { tag = TempItem[0] });
                        AspectMap.Add(TempItem[0], new());
                    }
                    else
                    {
                        AspectMap.Add(TempItem[0], [TempItem[1], TempItem[2]]);
                        if (AspectMap.ContainsKey(TempItem[1]))
                        {
                            if (!AspectMap[TempItem[1]].Contains(TempItem[0]))
                                AspectMap[TempItem[1]].Add(TempItem[0]);
                        }
                        else
                            AspectMap.Add(TempItem[1], [TempItem[0]]);

                        if (AspectMap.ContainsKey(TempItem[2]))
                        {
                            if (!AspectMap[TempItem[2]].Contains(TempItem[0]))
                                AspectMap[TempItem[2]].Add(TempItem[0]);
                        }
                        else
                            AspectMap.Add(TempItem[2], [TempItem[0]]);
                        AllAspect.Add(TempItem[0], new Aspect() { tag = TempItem[0], components = [AllAspect[TempItem[1]], AllAspect[TempItem[2]]] });
                    }
                }
                foreach (var item in Notes[0].Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var TempItem = item.Split(':');
                    //UserAspect.Add(AllAspect.FirstOrDefault(x => x.tag == TempItem[0]), int.Parse(TempItem[1]));
                    UserAspect.Add(TempItem[0], int.Parse(TempItem[1]));
                }
                Note = new();

                foreach (var item in Notes[2].Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var TempItem = item.Split(':');
                    var NoteKey = $"{TempItem[0]}:{TempItem[1]}";
                    Note.hexes.Add(NoteKey, new Hex(NoteKey));
                    if (TempItem.Length == 2)
                    {
                        Note.hexEntries.Add(NoteKey, new HexEntry() { });
                        //NoteHex.Add(key: $"{TempItem[0]}:{TempItem[1]}", (int.Parse(TempItem[0]), int.Parse(TempItem[1]), string.Empty));
                    }
                    else if (TempItem.Length == 3)
                    {
                        Note.hexEntries.Add(NoteKey, new HexEntry() { aspect = AllAspect[TempItem[2]] });
                        //NoteHex.Add(key: $"{TempItem[0]}:{TempItem[1]}", (int.Parse(TempItem[0]), int.Parse(TempItem[1]), TempItem[2]));
                    }
                }
                AutoResearchHandle();
                if (AllRetAspect.Count != 0)
                {
                    StringBuilder RetString = new StringBuilder();
                    foreach (var item in AllRetAspect)
                    {
                        RetString.Append(item.Key.q);
                        RetString.Append(":");
                        RetString.Append(item.Key.r);
                        RetString.Append("|");
                        RetString.Append(item.Value);
                        RetString.Append("&");
                    }
                    Console.WriteLine(RetString.ToString());
                }
                else
                {
                    Console.WriteLine("ResearchFail");
                }
            }
        }

        private static List<(string, HexEntry)> TargeItem = new List<(string, HexEntry)>();
        private static Dictionary<Hex, string> AllRetAspect = new Dictionary<Hex, string>();

        private static void AutoResearchHandle()
        {
            //Data = File.ReadAllText("WaitOpera.json");

            {
                foreach (var item in Note.hexEntries)
                {
                    if (item.Value.aspect != null)
                    {
                        TargeItem.Add((item.Key, item.Value));
                    }
                }
                if (TargeItem.Count < 4)
                {
                    while (true)
                    {
                        try
                        {
                            var TarGetPoint = Note.hexes["0:0"];
                            var AllPathWanna = new List<(string, Dictionary<Hex, List<string>>)>();

                            //1 分别寻找中心点距离和位置
                            foreach (var item in TargeItem)
                            {
                                var TryPathAspect = new Dictionary<Hex, List<string>>();
                                //1-1寻找当前位置四周相邻是否存在方块
                                var AllNeighbors = GetNeighborsToTarget(Note.hexes[item.Item1], TarGetPoint, Note.hexes);//当前顶点四周的点以及分别距离0,0的距离
                                                                                                                         //1-2已经获得原始点与0-0的坐标和距离,接下来，创建从起始点到0坐标的所有可能元素列
                                TryPathAspect.Add(AllNeighbors.First().Item1, AspectMap[item.Item2.aspect.tag]);//存入下一坐标，以及坐标中可能存在的元素
                                FindLineAndAspect(AllNeighbors.First().Item1, TarGetPoint, TryPathAspect);//寻找下一坐标
                                AllPathWanna.Add((item.Item1, TryPathAspect));
                            }
                            void FindLineAndAspect(Hex OriHex, Hex TarGetPoint, Dictionary<Hex, List<string>> TryPathAspect)
                            {
                                if (OriHex == TarGetPoint)
                                    return;
                                //1-2-1选择第一条
                                //1-2-2创建该坐标所有的可能元素
                                //1-2-3记录该坐标以及该坐标可能的元素
                                //1-2-4 遍历进行，除非到达目标节点
                                //1-2-5寻找下一位置四周相邻是否存在方块
                                var ALlNeighbors = GetNeighborsToTarget(OriHex, TarGetPoint, Note.hexes);//寻找下一坐标点
                                var NextPoint = ALlNeighbors.FirstOrDefault(x => !TryPathAspect.ContainsKey(x.Item1));//标记下一坐标点
                                var TempList = TryPathAspect[OriHex];
                                var NextPointAspectList = new List<string>();
                                //获得下一坐标点可能存在的所有元素
                                foreach (var item in TempList)
                                {
                                    var FindF = AspectMap.FirstOrDefault(x => x.Key == item);
                                    foreach (var item2 in FindF.Value)
                                    {
                                        if (!NextPointAspectList.Exists(x => x == item2))
                                        {
                                            NextPointAspectList.Add(item2);
                                        }
                                    }
                                }
                                TryPathAspect.Add(NextPoint.Item1, NextPointAspectList.Distinct().ToList());
                                FindLineAndAspect(NextPoint.Item1, TarGetPoint, TryPathAspect);
                            }
                            //2 已经寻找到三条链路分别到达0坐标的可能性，现在对三条链路的可能性分析开始
                            //2-1 首先对可能的0号链路进行总结求交际
                            var intersection = new HashSet<string>();
                            foreach (var item in AllPathWanna)
                            {
                                if (intersection.Count == 0)
                                    foreach (var item2 in item.Item2.Last().Value)
                                    {
                                        intersection.Add(item2);
                                    }
                                else
                                {
                                    intersection.IntersectWith(item.Item2.Last().Value);
                                }
                            }
                            //2-2 分情况处理
                            //2-2-1 如果能够找到共同交集，则生成基于该交集的连接网络，同时检查元素的储备情况

                            //链路检查，如果找到交叉点则打断

                            if (intersection.Count != 0)
                            {
                                var Solves = new List<(Dictionary<Hex, string>, Dictionary<string, int>, Dictionary<string, int>)>();
                                foreach (var item in intersection)
                                {
                                    Dictionary<Hex, string> RetAspect = new Dictionary<Hex, string>();
                                    RetAspect.Add(TarGetPoint, item);
                                    Dictionary<string, int> NoteAspectNum = new();
                                    foreach (var item2 in AllPathWanna)
                                    {
                                        var Ret = 限定路径最后一位元素归一化(item2.Item2, (TarGetPoint, item));
                                        foreach (var WaitAdd in Ret)
                                        {
                                            if (RetAspect.FirstOrDefault(x => x.Key == WaitAdd.Key).Key != null) continue;
                                            if (WaitAdd.Value.Count > 1)
                                            {
                                                var FindF = WaitAdd.Value.Where(key => UserAspect.ContainsKey(key))
                                                    .OrderByDescending(key => UserAspect[key])
                                                    .FirstOrDefault();
                                                RetAspect.Add(WaitAdd.Key, FindF);
                                                AddToNoteAspectNum(FindF);
                                            }
                                            else if (WaitAdd.Value.Count != 0)
                                            {
                                                RetAspect.Add(WaitAdd.Key, WaitAdd.Value.First());
                                                AddToNoteAspectNum(WaitAdd.Value.First());
                                            }
                                        }
                                    }
                                    var UnlessNum = new Dictionary<string, int>();
                                    foreach (var WaitHandler in NoteAspectNum.ToArray())
                                    {
                                        var Count = 0;
                                        if (UserAspect.ContainsKey(WaitHandler.Key))
                                            Count = UserAspect[WaitHandler.Key];
                                        if (WaitHandler.Value > Count)
                                        {
                                            FindNeed(WaitHandler.Key, WaitHandler.Value - Count);
                                        }
                                        void FindNeed(string AspectTag, int Num)
                                        {
                                            var GetC = AllAspect[AspectTag].components;
                                            if (GetC != null)
                                            {
                                                var Count1 = UserAspect[GetC[0].tag];
                                                var Count2 = UserAspect[GetC[1].tag];
                                                if (Count1 < Num)
                                                {
                                                    FindNeed(GetC[0].tag, Num);
                                                }
                                                if (Count2 < Num)
                                                {
                                                    FindNeed(GetC[1].tag, Num);
                                                }
                                            }
                                            else
                                            {
                                                if (UnlessNum.ContainsKey(AspectTag))
                                                    UnlessNum[AspectTag] += Num;
                                                else
                                                    UnlessNum.Add(AspectTag, Num);
                                            }
                                        }
                                    }

                                    Solves.Add((RetAspect, NoteAspectNum, UnlessNum));

                                    void AddToNoteAspectNum(string AddTempAspect)
                                    {
                                        if (NoteAspectNum.ContainsKey(AddTempAspect))
                                            NoteAspectNum[AddTempAspect] += 1;
                                        else
                                            NoteAspectNum.Add(AddTempAspect, 1);
                                    }
                                    void TryAddAspect(Aspect waitFind, int Num, Dictionary<string, int> SaveDic)
                                    {
                                        if (SaveDic.ContainsKey(waitFind.tag))
                                        {
                                            SaveDic[waitFind.tag] += Num;
                                        }
                                        else
                                        {
                                            SaveDic.Add(waitFind.tag, Num);
                                        }

                                        if (waitFind.components == null)
                                            return;

                                        var Find = UserAspect.FirstOrDefault(x => x.Key == waitFind.components[0].tag);
                                        if (Find.Key == null)
                                        {
                                            TryAddAspect(waitFind.components[0], Num, SaveDic);
                                        }
                                        else if (Find.Value < Num)
                                        {
                                            TryAddAspect(waitFind.components[0], Num, SaveDic);
                                        }
                                        Find = UserAspect.FirstOrDefault(x => x.Key == waitFind.components[1].tag);
                                        if (Find.Key == null)
                                        {
                                            TryAddAspect(waitFind.components[1], Num, SaveDic);
                                        }
                                        if (Find.Value < Num)
                                        {
                                            TryAddAspect(waitFind.components[1], Num, SaveDic);
                                        }
                                    }
                                }
                                var Find = Solves.FirstOrDefault(x => x.Item3.Count == 0);
                                if (Find.Item1 != null)
                                {
                                    AllRetAspect = Find.Item1;
                                }
                                else
                                {
                                    AllRetAspect = Solves
                                       .SelectMany(solve => solve.Item3.Keys.Distinct())
                                       .GroupBy(key => key)
                                       .Select(g => new
                                       {
                                           Key = g.Key,
                                           Count = Solves.Count(s => s.Item3.ContainsKey(g.Key)),
                                           MinValue = Solves
                                           .Where(s => s.Item3.ContainsKey(g.Key))
                                           .Select(s => s.Item3[g.Key])
                                           .Min()
                                       })
                                       .OrderBy(x => x.Count)
                                       .ThenBy(x => x.MinValue)
                                       .Select(x => x.Key)
                                       .Select(bestKey => Solves
                                       .Where(s => s.Item3.ContainsKey(bestKey))
                                       .OrderBy(s => s.Item3[bestKey])
                                       .Select(s => s.Item1)
                                       .First()).First();
                                }
                            }
                            else
                            {
                                var ExClude = Note.hexes.ToDictionary(a => a.Key, b => b.Value);
                                foreach (var item in TargeItem)
                                {
                                    ExClude.Remove(item.Item1);
                                }
                                var GetSolve = new Dictionary<Hex, string>();
                                //var GetSolveCount = new Dictionary<string, int>();
                                for (int i = 0, j = 1; i < TargeItem.Count; i++, j++)
                                {
                                    if (j == TargeItem.Count) j = 0;
                                    var FirstTag = TargeItem[i];
                                    var OrderList = TargeItem[j];
                                    var TryPathAspect = new Dictionary<Hex, List<string>>();

                                    获得指定路径可能所有解(FirstTag, OrderList, TryPathAspect, ExClude);//生成FirstTag到OrderList的所有可能解，但是OrderList存在所有可能性
                                    var NewTryPathAspect = 限定路径最后一位元素归一化(TryPathAspect, (Note.hexes[OrderList.Item1], OrderList.Item2.aspect.tag));//将OrderList限定为唯一解，并根据唯一解重新计算链路可能性

                                    if (NewTryPathAspect.Values.Any(list => list.Count == 0))//两项中找不到解，则增加一条链路
                                    {
                                        var GetOtherAspectPoint = TryPathAspect.Last().Key.GetNeighbors().Where(x => !TryPathAspect.ContainsKey(x) && Note.hexes.ContainsValue(x)).OrderBy(x => x.Distance(Note.hexes[FirstTag.Item1])).First();
                                        TryPathAspect = new Dictionary<Hex, List<string>>();
                                        获得指定路径可能所有解(FirstTag, (GetOtherAspectPoint.ToString(), new HexEntry()), TryPathAspect, ExClude);
                                        TryPathAspect.Add(Note.hexes[OrderList.Item1], [OrderList.Item2.aspect.tag]);
                                        NewTryPathAspect = 限定路径最后一位元素归一化(TryPathAspect, (Note.hexes[OrderList.Item1], OrderList.Item2.aspect.tag));
                                    }

                                    var OperaRet = new Dictionary<(string, string), Dictionary<Hex, List<string>>>();
                                    OperaRet.Add((FirstTag.Item1, OrderList.Item1), NewTryPathAspect);
                                    if (TargeItem.Count > 2)
                                    {
                                        for (int m = 0; m < TargeItem.Count; m++)//目前只能查询3个元素，检测4个元素以上的可能性
                                        {
                                            if (m == i || m == j) continue;
                                            FirstTag = TargeItem[m];
                                            foreach (var OrderList2 in NewTryPathAspect.OrderBy(x => x.Key.Distance(Note.hexes[FirstTag.Item1])))
                                            {
                                                TryPathAspect = new Dictionary<Hex, List<string>>();
                                                获得指定路径可能所有解(FirstTag, (OrderList2.Key.ToString(), Note.hexEntries[OrderList2.Key.ToString()]), TryPathAspect, ExClude);//生成FirstTag到OrderList2的所有可能解，但是OrderList2多种可能性，与之前已经存在的OrderList2求交集
                                                var NeedAspectList = OrderList2.Value.Intersect(TryPathAspect.Last().Value).ToList();
                                                //如果NeedAspectList的交集为空，则轮空寻找下一个节点的可能性
                                                if (NeedAspectList.Count == 0) continue;

                                                foreach (var item in NeedAspectList)//限定中间节点的唯一解
                                                {
                                                    NewTryPathAspect = 限定路径最后一位元素归一化(TryPathAspect, (OrderList2.Key, item));//将OrderList限定为唯一解，并根据唯一解重新计算链路可能性
                                                    OperaRet.Add((FirstTag.Item1, OrderList2.Key.ToString()), NewTryPathAspect);

                                                    foreach (var item2 in OperaRet.Values)
                                                    {
                                                        foreach (var item3 in 限定路径指定位置归一化(item2, (OrderList2.Key, item)))
                                                        {
                                                            if (!GetSolve.ContainsKey(item3.Key) && !TargeItem.Exists(x => x.Item1 == item3.Key.ToString()))
                                                            {
                                                                GetSolve.Add(item3.Key, item3.Value);
                                                                //if (GetSolveCount.ContainsKey(item3.Value))
                                                                //    GetSolveCount[item3.Value] += 1;
                                                                //else
                                                                //    GetSolveCount.Add(item3.Value, 1);
                                                            }
                                                        }
                                                    }
                                                    break;
                                                }
                                                if (GetSolve.Count != 0) break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var Ret = 计算获得最终解(TryPathAspect, (TryPathAspect.Last().Key, TryPathAspect.Last().Value.First()));
                                        AllRetAspect = Ret.Item1;
                                    }
                                    if (GetSolve.Count != 0)
                                    {
                                        break;
                                    }
                                }
                                if (GetSolve.Count != 0)
                                {
                                    AllRetAspect = GetSolve;

                                    //PostData = JsonSerializer.Serialize(new ResearchRet()
                                    //{
                                    //    Path = GetSolve.Select(x => new string[] { x.Key.ToString(), x.Value }).ToArray(),
                                    //    Aspect = GetSolveCount.Select(x => new string[] { x.Key, x.Value.ToString() }).ToArray()
                                    //});
                                    //PostCommand = "Research";
                                    break;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        break;
                    }
                }
                else
                {
                    //寻找两两之间，最长链路
                    var maxPair = TargeItem
                        .Select((item, i) => (
                        A: Note.hexes[item.Item1],
                        B: Note.hexes[TargeItem[(i + 1) % TargeItem.Count].Item1]))
                        .OrderByDescending(pair => pair.A.Distance(pair.B))
                        .First();
                    var FirstTag = TargeItem.First(x => x.Item1 == maxPair.A.ToString());
                    var OrderList = TargeItem.First(x => x.Item1 == maxPair.B.ToString());
                    var TryPathAspect = new Dictionary<Hex, List<string>>();

                    var ShortestPathRoot = FindShortestPathToChain(Note.hexes[FirstTag.Item1], [Note.hexes[OrderList.Item1]], Note.hexes.Values.ToList());

                    foreach (var path in ShortestPathRoot)
                    {
                        TryPathAspect.Add(path, new List<string>());
                    }
                    TryPathAspect.First().Value.Add(FirstTag.Item2.aspect.tag);

                    TryPathAspect.Last().Value.Add(OrderList.Item2.aspect.tag);
                    var GetSolve2 = 计算两点之间所有可能性2(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                    //获得指定路径可能所有解(FirstTag, OrderList, TryPathAspect, Note.hexes);//生成FirstTag到OrderList的所有可能解，但是OrderList存在所有可能性
                    //var NewTryPathAspect = 限定路径最后一位元素归一化(TryPathAspect, (Note.hexes[OrderList.Item1], OrderList.Item2.aspect.tag));//将OrderList限定为唯一解，并根据唯一解重新计算链路可能性

                    //计算链路所有可能解(NewTryPathAspect);
                    //List<((string, string), Dictionary<Hex, List<string>>)> WannaPath = new();
                    //for (int m = 0; m < TargeItem.Count; m++)
                    //{
                    //    var CurrectTag = TargeItem[m];
                    //    if (CurrectTag.Item1 == FirstTag.Item1 || CurrectTag.Item1 == OrderList.Item1) continue;
                    //    foreach (var OrderList2 in NewTryPathAspect.OrderBy(x => x.Key.Distance(Note.hexes[CurrectTag.Item1])))
                    //    {
                    //        TryPathAspect = new Dictionary<Hex, List<string>>();
                    //        获得指定路径可能所有解(CurrectTag, (OrderList2.Key.ToString(), Note.hexEntries[OrderList2.Key.ToString()]), TryPathAspect);
                    //        var NeedAspectList = OrderList2.Value.Intersect(TryPathAspect.Last().Value).ToList();
                    //        if (NeedAspectList.Count == 0) continue;
                    //        OrderList2.Value.Clear();
                    //        OrderList2.Value.AddRange(TryPathAspect.Last().Value);
                    //        WannaPath.Add(((CurrectTag.Item1, OrderList2.Key.ToString()), TryPathAspect));
                    //        break;
                    //    }
                    //}
                    List<Dictionary<Hex, string>> Solves = new();
                    List<Dictionary<Hex, string>> RetSolves = new();

                    //foreach (var item in 计算两点之间所有可能性(TryPathAspect.ToDictionary(), new Dictionary<Hex, string>(), 0))
                    foreach (var item in GetSolve2)
                    {
                        List<((string, string), Dictionary<Hex, string>)> TempWannaPath = new();
                        for (int m = 0; m < TargeItem.Count; m++)
                        {
                            var CurrectTag = TargeItem[m];
                            if (CurrectTag.Item1 == FirstTag.Item1 || CurrectTag.Item1 == OrderList.Item1) continue;
                            TryPathAspect = new Dictionary<Hex, List<string>>();

                            var ShortestPath = FindShortestPathToChain(Note.hexes[CurrectTag.Item1], item.Keys.ToList(), Note.hexes.Values.ToList());

                            foreach (var path in ShortestPath)
                            {
                                TryPathAspect.Add(path, new List<string>());
                            }
                            TryPathAspect.First().Value.Add(CurrectTag.Item2.aspect.tag);

                            var OrderList2 = item[TryPathAspect.Last().Key];

                            TryPathAspect.Last().Value.Add(OrderList2);

                            var GetSolve = 计算两点之间所有可能性2(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                            if (GetSolve.Length == 0)
                            {
                                goto End;
                            }
                            TempWannaPath.Add(((CurrectTag.Item1, OrderList2), GetSolve.First()));
                        }
                        foreach (var SaveToSolves in TempWannaPath)
                        {
                            foreach (var item2 in SaveToSolves.Item2)
                            {
                                if (!item.ContainsKey(item2.Key))
                                    item.Add(item2.Key, item2.Value);
                            }
                        }
                        foreach (var item2 in TargeItem)
                        {
                            item.Remove(Note.hexes[item2.Item1]);
                        }

                        var 元素耗费 = new Dictionary<string, int>();
                        foreach (var item2 in item)
                        {
                            if (!元素耗费.ContainsKey(item2.Value))
                            {
                                元素耗费.Add(item2.Value, 1);
                            }
                            else
                            {
                                元素耗费[item2.Value] += 1;
                            }
                        }
                        var RetNeed = new Dictionary<string, int>();

                        foreach (var item2 in 元素耗费)
                        {
                            var ApectAspectCheck = NeedCombine(item2.Key, item2.Value, RetNeed);
                            if (!NeedCombine(item2.Key, item2.Value, RetNeed))
                            {
                                goto End;
                            }
                        }
                        AllRetAspect = item;
                        goto Over;
                        bool NeedCombine(string WaitCombineASpect, int Count, Dictionary<string, int> SaveCount)
                        {
                            bool RetCheck = true;
                            if (UserAspect.ContainsKey(WaitCombineASpect))//值存在
                            {
                                if (UserAspect[WaitCombineASpect] - Count < 0)//但是值不足
                                {
                                    if (AllAspect[WaitCombineASpect].components != null)//并不是基础元素
                                    {
                                        foreach (var item in AllAspect[WaitCombineASpect].components)
                                        {
                                            RetCheck = NeedCombine(item.tag, Math.Abs(UserAspect[WaitCombineASpect] - Count), SaveCount);
                                        }
                                    }
                                    else//基础元素不足
                                    {
                                        RetCheck = false;
                                        AddToSaveCount();
                                    }
                                }
                                else
                                {
                                    //只存在且值足够
                                    AddToSaveCount();
                                }
                            }
                            else
                            {
                                if (AllAspect[WaitCombineASpect].components != null)
                                {
                                    foreach (var item in AllAspect[WaitCombineASpect].components)
                                    {
                                        RetCheck = NeedCombine(item.tag, Count, SaveCount);
                                    }
                                }
                                else
                                {
                                    RetCheck = false;
                                    AddToSaveCount();
                                }
                            }
                            return RetCheck;
                            void AddToSaveCount()
                            {
                                if (SaveCount.ContainsKey(WaitCombineASpect))//已经存在的基础元素+Count
                                {
                                    SaveCount[WaitCombineASpect] += Count;
                                }
                                else
                                    SaveCount.Add(WaitCombineASpect, Count);
                            }
                        }

                    End:
                        continue;
                    }
                Over:
                    {
                    }
                    //统计所有解算中，元素耗费

                    //对可能得解进行降维处理

                    //try
                    //{
                    //    var TempNewTryPathAspect = NewTryPathAspect.ToDictionary(
                    //        entry => entry.Key,
                    //        entry => new List<string>(entry.Value));
                    //    foreach (var item in WannaPath)
                    //    {
                    //        TempNewTryPathAspect[Note.hexes[item.Item1.Item2]].Clear();
                    //        TempNewTryPathAspect[Note.hexes[item.Item1.Item2]].AddRange(item.Item2.Last().Value);

                    // var list = TempNewTryPathAspect.ToList(); // 将字典转换为列表，保留顺序 int centerIndex =
                    // list.FindIndex(kv => kv.Key == Note.hexes[item.Item1.Item2]);

                    // var TempInn = new List<string>(item.Item2.Last().Value); for (int i =
                    // centerIndex + 1; i < list.Count; i++) { var pair = list[i]; var TempL =
                    // pair.Value.Intersect(GetMapAspectList(TempInn)).ToList(); pair.Value.Clear();
                    // pair.Value.AddRange(TempL); TempInn = TempL; } TempInn = new List<string>(item.Item2.Last().Value);

                    // for (int i = centerIndex - 1; i >= 0; i--) { var pair = list[i]; var TempL =
                    // pair.Value.Intersect(GetMapAspectList(TempInn)).ToList(); pair.Value.Clear();
                    // pair.Value.AddRange(TempL); TempInn = TempL; } List<string>
                    // GetMapAspectList(List<string> OriList) { var MapInn = new List<string>();
                    // foreach (var item2 in TempInn) { MapInn.AddRange(AspectMap[item2]); } return
                    // MapInn.Distinct().ToList(); } }

                    //    对降维完毕的链路计算所有可能性(TempNewTryPathAspect, WannaPath, SaveList, 0);
                    //}
                    //catch (Exception)
                    //{
                    //}
                }
            }
            return;
        }

        //(bool, Dictionary<Hex, string>)

        private static IEnumerable<Dictionary<Hex, string>> 计算两点之间所有可能性(Dictionary<Hex, List<string>> OriList, Dictionary<Hex, string> RetSaveList, int CurretCount)
        {
            if (CurretCount >= OriList.Count)
            {
                // 到达叶子节点，返回完整路径（必须复制 RetSaveList）
                yield return new Dictionary<Hex, string>(RetSaveList);
                yield break;
            }
            var CurrectHex = OriList.ElementAt(CurretCount).Key;
            //这里应该是基于上一级的下一级可能解
            var ForeachList = new List<string>();
            if (RetSaveList.Count == 0)
            {
                ForeachList.AddRange(OriList.ElementAt(CurretCount).Value);
            }
            else
            {
                foreach (var item in AspectMap[RetSaveList[OriList.ElementAt(CurretCount - 1).Key]])
                {
                    ForeachList.Add(item);
                }
                ForeachList = ForeachList.Intersect(OriList.ElementAt(CurretCount).Value).ToList();
            }
            CurretCount += 1;

            foreach (var item in ForeachList)
            {
                var SaveList = new Dictionary<Hex, string>();
                foreach (var Saveitem in RetSaveList)
                {
                    SaveList.Add(Saveitem.Key, Saveitem.Value);
                }
                SaveList.Add(CurrectHex, item);

                //if (TargetList.Exists(x => x.Item1.Item2 == CurrectHex.ToString()))
                //{
                //    foreach (var FindItem in TargetList.FindAll(x => x.Item1.Item2 == CurrectHex.ToString()))
                //    {
                //        var TryPath = 限定路径指定位置归一化(FindItem.Item2, (CurrectHex, item));
                //        if (TryPath.Count == 0) return false;
                //        foreach (var Saveitem in TryPath)
                //        {
                //            if (!SaveList.ContainsKey(Saveitem.Key))
                //                SaveList.Add(Saveitem.Key, Saveitem.Value);
                //        }
                //    }
                //    var Ret = 对降维完毕的链路计算所有可能性(OriList, TargetList, SaveList, CurretCount);
                //}
                //else
                //{
                //    var Ret = 对降维完毕的链路计算所有可能性(OriList, TargetList, SaveList, CurretCount);
                //}

                //yield return 计算两点之间所有可能性(OriList, SaveList, CurretCount).ToImmutableArray();
                // 复制 RetSaveList，添加当前值

                // 递归调用，并逐一 yield return 子结果
                foreach (var result in 计算两点之间所有可能性(OriList, SaveList, CurretCount))
                {
                    yield return result;
                }
            }
        }

        private static IEnumerable<Dictionary<Hex, string>> 计算两点之间所有可能性2(Dictionary<Hex, List<string>> OriList, Dictionary<Hex, string> RetSaveList, int CurretCount)
        {
            if (CurretCount >= OriList.Count)
            {
                if (RetSaveList.Last().Value == OriList.Last().Value.First())
                    yield return new Dictionary<Hex, string>(RetSaveList);

                yield break;
            }
            var CurrectHex = OriList.ElementAt(CurretCount).Key;
            var ForeachList = new List<string>();
            if (RetSaveList.Count == 0)
            {
                ForeachList.AddRange(OriList.ElementAt(CurretCount).Value);
            }
            else
            {
                foreach (var item in AspectMap[RetSaveList[OriList.ElementAt(CurretCount - 1).Key]])
                {
                    ForeachList.Add(item);
                }
            }
            CurretCount += 1;

            foreach (var item in ForeachList)
            {
                var SaveList = new Dictionary<Hex, string>();
                foreach (var Saveitem in RetSaveList)
                {
                    SaveList.Add(Saveitem.Key, Saveitem.Value);
                }
                SaveList.Add(CurrectHex, item);
                foreach (var result in 计算两点之间所有可能性2(OriList, SaveList, CurretCount))
                {
                    yield return result;
                }
            }
        }

        private static List<(Hex, int)> GetNeighborsToTarget(Hex Orihex, Hex Target, Dictionary<string, Hex> ExitsHex)
        {
            Hex[] HexDirections =
                {
                new Hex{ q=1,r=0},
                new Hex{ q=1,r=-1},
                new Hex{ q=0,r=-1},
                new Hex{ q=-1,r=0},
                new Hex{ q=-1,r=1},
                new Hex{ q=0,r=1},
            };
            List<(Hex, int)> RetList = new();
            foreach (var dir in HexDirections)
            {
                var RetHex = $"{Orihex.q + dir.q}:{Orihex.r + dir.r}";
                if (ExitsHex.ContainsKey(RetHex))
                    RetList.Add((ExitsHex[RetHex], HexDistance(ExitsHex[RetHex], Target)));
            }
            return RetList.OrderBy(x => x.Item2).ToList();
            int HexDistance(Hex a, Hex b)
            {
                return (Math.Abs(a.q - b.q) + Math.Abs(a.q + a.r - b.q - b.r) + Math.Abs(a.r - b.r)) / 2;
            }
        }

        private static void FindLineAndAspect(Hex OriHex, Hex TarGetPoint, Dictionary<Hex, List<string>> TryPathAspect)
        {
            if (OriHex == TarGetPoint)
                return;
            //1-2-1选择第一条
            //1-2-2创建该坐标所有的可能元素
            //1-2-3记录该坐标以及该坐标可能的元素
            //1-2-4 遍历进行，除非到达目标节点
            //1-2-5寻找下一位置四周相邻是否存在方块
            var ALlNeighbors = GetNeighborsToTarget(OriHex, TarGetPoint, Note.hexes);//寻找下一坐标点
            var NextPoint = ALlNeighbors.FirstOrDefault(x => !TryPathAspect.ContainsKey(x.Item1));//标记下一坐标点
            var TempList = TryPathAspect[OriHex];
            var NextPointAspectList = new List<string>();
            //获得下一坐标点可能存在的所有元素
            foreach (var item in TempList)
            {
                var FindF = AspectMap.FirstOrDefault(x => x.Key == item);
                foreach (var item2 in FindF.Value)
                {
                    if (!NextPointAspectList.Exists(x => x == item2))
                    {
                        NextPointAspectList.Add(item2);
                    }
                }
            }
            TryPathAspect.Add(NextPoint.Item1, NextPointAspectList.Distinct().ToList());
            FindLineAndAspect(NextPoint.Item1, TarGetPoint, TryPathAspect);
        }

        private static (Dictionary<Hex, string>, Dictionary<string, int>) 计算获得最终解(Dictionary<Hex, List<string>> TryPathAspect, (Hex, string) TargetASpect)
        {
            var GetSolve = new Dictionary<Hex, string>();
            var GetSolveCount = new Dictionary<string, int>();
            foreach (var item3 in 限定路径指定位置归一化(TryPathAspect, (TryPathAspect.Last().Key, TryPathAspect.Last().Value.First())))
            {
                if (!GetSolve.ContainsKey(item3.Key) && !TargeItem.Exists(x => x.Item1 == item3.Key.ToString()))
                {
                    GetSolve.Add(item3.Key, item3.Value);
                    if (GetSolveCount.ContainsKey(item3.Value))
                        GetSolveCount[item3.Value] += 1;
                    else
                        GetSolveCount.Add(item3.Value, 1);
                }
            }
            return (GetSolve, GetSolveCount);
        }

        private static bool 获得指定路径可能所有解((string, HexEntry) OriTag, (string, HexEntry) TargetTag, Dictionary<Hex, List<string>> TryPathAspect, Dictionary<string, Hex> Exclude)
        {
            TryPathAspect.Add(Note.hexes[OriTag.Item1], [OriTag.Item2.aspect.tag]);
            var AllNeighbors = GetNeighborsToTarget(Note.hexes[OriTag.Item1], Note.hexes[TargetTag.Item1], Exclude);
            TryPathAspect.Add(AllNeighbors.First().Item1, AspectMap[OriTag.Item2.aspect.tag].Distinct().ToList());
            FindLineAndAspect(AllNeighbors.First().Item1, Note.hexes[TargetTag.Item1], TryPathAspect);
            return true;
        }

        private static Dictionary<Hex, List<string>> 限定路径最后一位元素归一化(Dictionary<Hex, List<string>> TryPathAspect, (Hex, string) LastAspectItem)
        {
            TryPathAspect.Last().Value.Clear();
            TryPathAspect.Last().Value.Add(LastAspectItem.Item2);
            var NewTryPathAspect = new Dictionary<Hex, List<string>>();
            NewTryPathAspect.Add(LastAspectItem.Item1, [LastAspectItem.Item2]);
            var PreAspectList = new List<string> { LastAspectItem.Item2 };
            foreach (var item in TryPathAspect.Reverse().Skip(1))
            {
                var NeedAspectList = new List<string>();
                foreach (var item2 in PreAspectList)
                {
                    NeedAspectList.AddRange(AspectMap[item2]);
                }
                var GetAspectList = item.Value.Intersect(NeedAspectList).ToList();
                NewTryPathAspect.Add(item.Key, GetAspectList);
                PreAspectList = GetAspectList;
            }
            return NewTryPathAspect;
        }

        private static Dictionary<Hex, string> 限定路径指定位置归一化(Dictionary<Hex, List<string>> TryPathAspect, (Hex, string) TargetASpect)
        {
            var orderedList = TryPathAspect.ToList();
            int index = orderedList.FindIndex(kv =>
                kv.Key.Equals(TargetASpect.Item1));

            Dictionary<Hex, string>? NewAdd = new Dictionary<Hex, string>();
            NewAdd.Add(TargetASpect.Item1, TargetASpect.Item2);
            var Currect = AspectMap[TargetASpect.Item2];
            for (int i = index + 1; i < orderedList.Count; i++)
            {
                var First = string.Empty;
                var possibiList = orderedList[i].Value.Intersect(Currect).ToList();
                if (possibiList.Count > 1)
                {
                    var sortedPossibiList = possibiList
                        .OrderByDescending(s => UserAspect.ContainsKey(s) ? UserAspect[s] : 0)
                        .ToList();
                    var topN = sortedPossibiList.Take(sortedPossibiList.Count / 2).ToList(); // 可选：只从前几个中抽取
                    First = topN[new Random().Next(sortedPossibiList.Count / 2)];
                }
                else
                    First = possibiList.First();
                NewAdd.Add(orderedList[i].Key, First);
                Currect = AspectMap[First];
            }
            Dictionary<Hex, string> OldAdd = new Dictionary<Hex, string>();
            Currect = AspectMap[TargetASpect.Item2];
            for (int i = index - 1; i >= 0; i--)
            {
                var First = orderedList[i].Value.Intersect(Currect).FirstOrDefault();//这里可以匹配用户存有的最大值，现在使用随机算法
                if (string.IsNullOrEmpty(First))
                {
                    return new();
                }
                OldAdd.Add(orderedList[i].Key, First);
                Currect = AspectMap[First];
            }
            return OldAdd.Concat(NewAdd).GroupBy(kv => kv.Key).ToDictionary(g => g.Key, g => g.Last().Value); ;
        }

        private static Dictionary<Hex, string> 首尾确定路径归一化(Dictionary<Hex, List<string>> TryPathAspect)
        {
            var orderedList = TryPathAspect.ToList();

            Dictionary<Hex, string>? NewAdd = new Dictionary<Hex, string>();

            var LastAspect = "";
            for (int i = 0; i < TryPathAspect.Count; i++)
            {
                var CurrectItem = TryPathAspect.ElementAt(i);
                if (i == 0)
                {
                    LastAspect = CurrectItem.Value.FirstOrDefault();
                    NewAdd.Add(CurrectItem.Key, LastAspect);
                }
                var possibiList = AspectMap[LastAspect];
                var sortedPossibiList = possibiList.OrderByDescending(s => UserAspect.ContainsKey(s) ? UserAspect[s] : 0).ToList();
            }
            foreach (var item in TryPathAspect)
            {
                if (item.Value.Count == 1)
                {
                    NewAdd.Add(item.Key, item.Value.First());
                    LastAspect = item.Value.First();
                    continue;
                }
            }

            foreach (var item in TryPathAspect.Reverse())
            {
                if (item.Value.Count == 1)
                {
                    NewAdd.Add(item.Key, item.Value.First());
                    LastAspect = item.Value.First();
                    continue;
                }
                var possibiList = AspectMap[LastAspect];

                var sortedPossibiList = possibiList
                       .OrderByDescending(s => UserAspect.ContainsKey(s) ? UserAspect[s] : 0)
                       .ToList();
                var topN = sortedPossibiList.Take(sortedPossibiList.Count / 2).ToList(); // 可选：只从前几个中抽取
                LastAspect = topN[new Random().Next(sortedPossibiList.Count / 2)];
                NewAdd.Add(item.Key, LastAspect);
            }

            return NewAdd;
        }

        private static List<Hex> FindShortestPathToChain(Hex start, List<Hex> chain, List<Hex> walkable)
        {
            var walkableSet = new HashSet<Hex>(walkable);
            var targetSet = new HashSet<Hex>(chain);

            var visited = new HashSet<Hex>();
            var cameFrom = new Dictionary<Hex, Hex>();
            var queue = new Queue<Hex>();

            queue.Enqueue(start);
            visited.Add(start);

            Hex end = null;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (targetSet.Contains(current))
                {
                    end = current;
                    break;
                }

                foreach (var neighbor in current.GetNeighbors())
                {
                    if (!visited.Contains(neighbor) && walkableSet.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (end == null)
                return null; // 没有路径

            // 回溯路径
            var path = new List<Hex>();
            var cur = end;
            while (!cur.Equals(start))
            {
                path.Add(cur);
                cur = cameFrom[cur];
            }
            path.Add(start);
            path.Reverse();
            return path;
        }
    }
}
