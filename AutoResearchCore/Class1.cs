using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AutoResearch.Class1;
using static AutoResearch.Program;

namespace AutoResearch
{
    public class Hex
    {
        public Hex()
        {
        }

        public Hex(int Q, int R)
        {
            q = Q; r = R;
        }

        public Hex(string item1)
        {
            var sp = item1.Split(':');
            q = int.Parse(sp[0]);
            r = int.Parse(sp[1]);
        }

        public override bool Equals(object obj)
        {
            if (obj is not Hex other) return false;
            return q == other.q && r == other.r;
        }

        public override int GetHashCode() => HashCode.Combine(q, r);

        public static int getDistance(Hex a1, Hex a2)
        {
            return (Math.Abs(a1.q - a2.q) + Math.Abs(a1.r - a2.r) + Math.Abs(a1.q + a1.r - a2.q - a2.r)) / 2;
        }

        public int Distance(Hex a2)
        {
            return (Math.Abs(q - a2.q) + Math.Abs(r - a2.r) + Math.Abs(q + r - a2.q - a2.r)) / 2;
        }

        public int q { get; set; } = 0;
        public int r { get; set; } = 0;

        public override string ToString()
        {
            return $"{q}:{r}";
        }

        internal IEnumerable<Hex> GetNeighbors()
        {
            return new List<Hex>
            {
                new Hex(q + 1, r),     // 右
                new Hex(q - 1, r),     // 左
                new Hex(q, r + 1),     // 右上
                new Hex(q, r - 1),     // 左下
                new Hex(q + 1, r - 1), // 右下
                new Hex(q - 1, r + 1)  // 左上
            };
        }
    }

    internal class Class1
    {
        private static Dictionary<string, (string, string)> AllAspect = new();
        private static Dictionary<Hex, string> NoteHex = new();
        private static List<Hex> Hexes = new();

        private static Dictionary<string, List<string>> AspectMap = new();
        private static Dictionary<string, int> UserAspect = new();
        private static Dictionary<Hex, string> TargeItem = new();

        private static string[] Notes;

        private static async Task<int> Main(string[] args)
        {
            //Debugger.Launch();
            //Debugger.Break();
            //if (args.Length != 0)
            //    File.WriteAllText("Note.txt", args[0]);
            //if (File.Exists("Note.txt"))
            //    Notes = File.ReadAllText($@"Note.txt").Split('^');
            //else if (File.Exists($@"..\..\..\..\Note.txt"))
            //    Notes = File.ReadAllText($@"..\..\..\..\Note.txt").Split('^');
            if (args.Length == 1)
                Notes = args[0].Split('^');
            if (Notes.Length == 3)
            {
                foreach (var item in Notes[1].Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var TempItem = item.Split(':');
                    if (TempItem.Length == 1)
                    {
                        AllAspect.Add(TempItem[0], (string.Empty, string.Empty));
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
                        AllAspect.Add(TempItem[0], (TempItem[1], TempItem[2]));
                    }
                }
                foreach (var item in Notes[0].Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var TempItem = item.Split(':');
                    UserAspect.Add(TempItem[0], int.Parse(TempItem[1]));
                }
                foreach (var item in Notes[2].Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var TempItem = item.Split(':');
                    //var NoteKey = $"{TempItem[0]}:{TempItem[1]}";
                    var WaitAddHex = new Hex() { q = int.Parse(TempItem[0]), r = int.Parse(TempItem[1]) };
                    Hexes.Add(WaitAddHex);
                    if (TempItem.Length == 2)
                    {
                        NoteHex.Add(WaitAddHex, string.Empty);
                    }
                    else if (TempItem.Length == 3)
                    {
                        NoteHex.Add(WaitAddHex, TempItem[2]);
                    }
                }
                foreach (var item in NoteHex)
                {
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        TargeItem.Add(item.Key, item.Value);
                    }
                }
                var Solves = new ConcurrentBag<Dictionary<Hex, string>>();

                var cts = new CancellationTokenSource();
                var token = cts.Token;

                object solvesLock = new object();
                bool timerStarted = false;

                var T1 = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        Dictionary<Hex, string> Solver = new();
                        var Pair = TargeItem.SelectMany((itemA, i) => TargeItem.Skip(i + 1)
                        .Select(itemB => (A: itemA, B: itemB, Distance: itemA.Key.Distance(itemB.Key))))
                        .OrderByDescending(pair => pair.Distance).ToList();
                        foreach (var maxPair in Pair)
                        {
                            var ShortestPathRoot = FindShortestPathToChain(maxPair.A.Key, [maxPair.B.Key], Hexes);
                            var TryPathAspect = new Dictionary<Hex, List<string>>();
                            foreach (var item in ShortestPathRoot)
                            {
                                TryPathAspect.Add(item, []);
                            }
                            TryPathAspect[maxPair.A.Key].Add(maxPair.A.Value);
                            TryPathAspect[maxPair.B.Key].Add(maxPair.B.Value);
                            Solver = GetPossibleSolve(GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray());
                            if (Solver.Count != 0)
                            {
                                Solves.Add(Solver);
                                cts.Cancel();
                                break;
                            }
                            if (token.IsCancellationRequested) break;
                        }
                        cts.Cancel();
                    }
                }, token);
                var T2 = Task.Run(() =>
                {
                    Dictionary<Hex, string> Solver = new();
                    var Pair = TargeItem
                    .Select((item, i) => (
                    A: item,
                    B: TargeItem.ElementAt((i + 1) % TargeItem.Count)))
                    .OrderByDescending(pair => pair.A.Key.Distance(pair.B.Key)).ToList();
                    foreach (var maxPair in Pair)
                    {
                        var ShortestPathRoot = FindShortestPathToChain(maxPair.A.Key, [maxPair.B.Key], Hexes);
                        var TryPathAspect = new Dictionary<Hex, List<string>>();
                        foreach (var item in ShortestPathRoot)
                        {
                            TryPathAspect.Add(item, []);
                        }
                        TryPathAspect[maxPair.A.Key].Add(maxPair.A.Value);
                        TryPathAspect[maxPair.B.Key].Add(maxPair.B.Value);
                        Solver = GetPossibleSolve(GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray());
                        if (Solver.Count != 0)
                        {
                            Solves.Add(Solver);
                            cts.Cancel();
                            break;
                        }
                        if (token.IsCancellationRequested) break;
                    }
                    cts.Cancel();
                }, token);
                var T3 = Task.Run(() =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        var WaitDelPoint = new List<Hex>();
                        var Solvess = new List<Dictionary<Hex, string>>();
                        var center = new Hex(0, 0);
                        var sortedTargeItem = TargeItem.OrderBy(kv =>
                        {
                            var dq = kv.Key.q - center.q;
                            var dr = kv.Key.r - center.r;
                            // 将轴向坐标换算为笛卡尔坐标（x,y），假设是平顶（pointy top）布局
                            double x = dq * Math.Sqrt(3) + dr * Math.Sqrt(3) / 2;
                            double y = dr * 1.5;
                            // 顺时针排序：负 atan2 角度
                            double angle = Math.Atan2(y, x); // 范围 [-π, π]
                            return (angle + 2 * Math.PI) % (2 * Math.PI); // 转为正角度 [0, 2π)
                        }).ToList();
                        for (int i = 0, j = 1; i < sortedTargeItem.Count; i++, j++)
                        {
                            if (j == sortedTargeItem.Count - 1 && Solvess.Count == sortedTargeItem.Count - 1)
                            {
                                break;
                            }
                            if (j == sortedTargeItem.Count)
                                j = 0;
                            var First = sortedTargeItem.ElementAt(i);
                            var Second = sortedTargeItem.ElementAt(j);
                            var IncludePoint = new List<Hex>(Hexes);
                            //foreach (var item in WaitDelPoint)
                            //{
                            //    IncludePoint.Remove(item);
                            //}
                            var ShortestPathRoot = FindShortestPathToChain(First.Key, [Second.Key], Hexes);
                            var TryPathAspect = new Dictionary<Hex, List<string>>();
                            foreach (var item in ShortestPathRoot)
                            {
                                TryPathAspect.Add(item, []);
                            }
                            TryPathAspect[First.Key].Add(First.Value);
                            TryPathAspect[Second.Key].Add(Second.Value);
                            int ReTryCount = 0;
                        ReTry:
                            var SubPossible = GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                            if (SubPossible.Length == 0)
                            {
                                ReTryCount += 1;

                                if (ReTryCount == 1)
                                {
                                    var anyHex = TryPathAspect.First();
                                    var newHex = anyHex.Key.GetNeighbors().Where(n => Hexes.Contains(n) && !TryPathAspect.ContainsKey(n)).OrderBy(x => x.Distance(TryPathAspect.Last().Key)).First();
                                    if (newHex != null)
                                    {
                                        var TempAspect = new Dictionary<Hex, List<string>>();
                                        TempAspect.Add(anyHex.Key, anyHex.Value);
                                        TempAspect.Add(newHex, anyHex.Value);
                                        for (int m = 1; m < TryPathAspect.Count; m++)
                                        {
                                            TempAspect.Add(TryPathAspect.ElementAt(m).Key, TryPathAspect.ElementAt(m).Value);
                                        }
                                        TryPathAspect = TempAspect;
                                        goto ReTry;
                                    }
                                }
                                else if (ReTryCount == 2)
                                {
                                    var anyHex = TryPathAspect.Last();
                                    var newHex = anyHex.Key.GetNeighbors()
                                    .Where(n => Hexes.Contains(n) && !TryPathAspect.ContainsKey(n)).OrderBy(x => x.Distance(TryPathAspect.First().Key)).First();
                                    if (newHex != null)
                                    {
                                        var TempAspect = new Dictionary<Hex, List<string>>();

                                        for (int m = 0; m < TryPathAspect.Count - 1; m++)
                                        {
                                            TempAspect.Add(TryPathAspect.ElementAt(m).Key, TryPathAspect.ElementAt(m).Value);
                                        }
                                        TempAspect.Add(newHex, anyHex.Value);
                                        TempAspect.Add(anyHex.Key, anyHex.Value);
                                        TryPathAspect = TempAspect;
                                        goto ReTry;
                                    }
                                }
                                else if (ReTryCount == 3)
                                {
                                }
                            }
                            else
                            {
                                foreach (var item in SubPossible.First())
                                {
                                    WaitDelPoint.Add(item.Key);
                                }
                                Solvess.Add(SubPossible.First());
                            }
                        }
                        if (Solvess.Count > sortedTargeItem.Count - 1)
                        {
                            Dictionary<Hex, string> Solver = new();

                            foreach (var item in Solvess)
                            {
                                foreach (var item2 in item)
                                {
                                    if (TargeItem.ContainsKey(item2.Key)) continue;

                                    if (Solver.ContainsKey(item2.Key) && Solver[item2.Key] != item2.Value)
                                    {
                                        Solver.Clear();
                                        goto End;
                                    }
                                    else if (!Solver.ContainsKey(item2.Key))
                                    {
                                        Solver.Add(item2.Key, item2.Value);
                                    }
                                }
                            }
                            Solves.Add(Solver);
                        }
                    End:
                        cts.Cancel();
                        break;
                    }
                }, token);

                try
                {
                    await Task.WhenAny(T1, T2, T3);
                }
                catch (OperationCanceledException)
                {
                }
                var Retry = 0;
                Dictionary<Hex, string> Solver = new();
            Retry:
                await Task.Delay(1000);
                if (Solves.Count == 1)
                {
                    Solver = Solves.First();
                }
                else if (Solves.Count > 1)
                {
                    Solver = Solves.OrderBy(solver => solver.Values.Distinct().Count()).First();
                }
                else if (Solves.Count == 0)
                {
                    if (Retry < 2)
                    {
                        Retry += 1;
                        goto Retry;
                    }
                }

                //var Pair = TargeItem
                //    .Select((item, i) => (
                //    A: item,
                //    B: TargeItem.ElementAt((i + 1) % TargeItem.Count)))
                //    .OrderByDescending(pair => pair.A.Key.Distance(pair.B.Key));
                //var Pair = TargeItem
                //    .SelectMany((itemA, i) =>
                //    TargeItem.Skip(i + 1).Select(itemB => (
                //    A: itemA,
                //    B: itemB,
                //    Distance: itemA.Key.Distance(itemB.Key))))
                //    .OrderByDescending(pair => pair.Distance).ToList();
                //var Pair2 = TargeItem
                //  .Select((item, i) => (
                //  A: item,
                //  B: TargeItem.ElementAt((i + 1) % TargeItem.Count)))
                //  .OrderByDescending(pair => pair.A.Key.Distance(pair.B.Key)).ToList();
                //var PaMerge = new List<(KeyValuePair<Hex, string> A, KeyValuePair<Hex, string> B)>();

                //int count = Math.Max(Pair.Count, Pair2.Count);
                //for (int i = 0; i < count; i++)
                //{
                //    if (PaMerge.Exists(x => x.A.Key == Pair[i].A.Key && x.B.Key == Pair[i].B.Key)) continue;
                //    if (PaMerge.Exists(x => x.A.Key == Pair[i].B.Key && x.B.Key == Pair[i].A.Key)) continue;
                //    if (i < Pair.Count)
                //        PaMerge.Add((Pair[i].A, Pair[i].B));

                //    if (i < Pair2.Count)
                //    {
                //        if (PaMerge.Exists(x => x.A.Key == Pair2[i].A.Key && x.B.Key == Pair2[i].B.Key)) continue;
                //        if (PaMerge.Exists(x => x.A.Key == Pair2[i].B.Key && x.B.Key == Pair2[i].A.Key)) continue;
                //        PaMerge.Add(Pair2[i]);
                //    }
                //}
                //var Solves = new ConcurrentBag<Dictionary<Hex, string>>();
                //Parallel.ForEach(PaMerge, maxPair =>
                //{
                //    var ShortestPathRoot = FindShortestPathToChain(maxPair.A.Key, [maxPair.B.Key], Hexes);
                //    var TryPathAspect = new Dictionary<Hex, List<string>>();
                //    foreach (var item in ShortestPathRoot)
                //    {
                //        TryPathAspect.Add(item, []);
                //    }
                //    TryPathAspect[maxPair.A.Key].Add(maxPair.A.Value);
                //    TryPathAspect[maxPair.B.Key].Add(maxPair.B.Value);
                //    var SubSolver = GetPossibleSolve(GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray());
                //    if (SubSolver.Count != 0)
                //        Solves.Add(SubSolver);
                //});

                //foreach (var maxPair in PaMerge)
                //{
                //    var ShortestPathRoot = FindShortestPathToChain(maxPair.A.Key, [maxPair.B.Key], Hexes);
                //    var TryPathAspect = new Dictionary<Hex, List<string>>();
                //    foreach (var item in ShortestPathRoot)
                //    {
                //        TryPathAspect.Add(item, []);
                //    }
                //    TryPathAspect[maxPair.A.Key].Add(maxPair.A.Value);
                //    TryPathAspect[maxPair.B.Key].Add(maxPair.B.Value);
                //    Solver = GetPossibleSolve(GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray());
                //    if (Solver.Count != 0)
                //        goto Over;
                //}
                //if (Solver.Count == 0)
                //{
                //    var Pair2 = TargeItem
                //    .Select((item, i) => (
                //    A: item,
                //    B: TargeItem.ElementAt((i + 1) % TargeItem.Count)))
                //    .OrderByDescending(pair => pair.A.Key.Distance(pair.B.Key));
                //    foreach (var maxPair in Pair2)
                //    {
                //        var ShortestPathRoot = FindShortestPathToChain(maxPair.A.Key, [maxPair.B.Key], Hexes);
                //        var TryPathAspect = new Dictionary<Hex, List<string>>();
                //        foreach (var item in ShortestPathRoot)
                //        {
                //            TryPathAspect.Add(item, []);
                //        }
                //        TryPathAspect[maxPair.A.Key].Add(maxPair.A.Value);
                //        TryPathAspect[maxPair.B.Key].Add(maxPair.B.Value);
                //        Solver = GetPossibleSolve(GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray());
                //        if (Solver.Count != 0)
                //            goto Over;
                //    }
                //}
                //Over:
                //    if (Solver.Count == 0)
                //    {
                //        var WaitDelPoint = new List<Hex>();
                //        var Solvess = new List<Dictionary<Hex, string>>();
                //        var center = new Hex(0, 0);
                //        var sortedTargeItem = TargeItem.OrderBy(kv =>
                //        {
                //            var dq = kv.Key.q - center.q;
                //            var dr = kv.Key.r - center.r;
                //            // 将轴向坐标换算为笛卡尔坐标（x,y），假设是平顶（pointy top）布局
                //            double x = dq * Math.Sqrt(3) + dr * Math.Sqrt(3) / 2;
                //            double y = dr * 1.5;
                //            // 顺时针排序：负 atan2 角度
                //            double angle = Math.Atan2(y, x); // 范围 [-π, π]
                //            return (angle + 2 * Math.PI) % (2 * Math.PI); // 转为正角度 [0, 2π)
                //        }).ToList();
                //        for (int i = 0, j = 1; i < sortedTargeItem.Count; i++, j++)
                //        {
                //            if (j == sortedTargeItem.Count - 1 && Solver.Count == sortedTargeItem.Count - 1)
                //            {
                //                break;
                //            }
                //            if (j == sortedTargeItem.Count)
                //                j = 0;
                //            var First = sortedTargeItem.ElementAt(i);
                //            var Second = sortedTargeItem.ElementAt(j);
                //            var IncludePoint = new List<Hex>(Hexes);
                //            //foreach (var item in WaitDelPoint)
                //            //{
                //            //    IncludePoint.Remove(item);
                //            //}
                //            var ShortestPathRoot = FindShortestPathToChain(First.Key, [Second.Key], Hexes);
                //            var TryPathAspect = new Dictionary<Hex, List<string>>();
                //            foreach (var item in ShortestPathRoot)
                //            {
                //                TryPathAspect.Add(item, []);
                //            }
                //            TryPathAspect[First.Key].Add(First.Value);
                //            TryPathAspect[Second.Key].Add(Second.Value);
                //            int ReTryCount = 0;
                //        ReTry:
                //            var SubPossible = GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                //            if (SubPossible.Length == 0)
                //            {
                //                ReTryCount += 1;

                // if (ReTryCount == 1) { var anyHex = TryPathAspect.First(); var newHex =
                // anyHex.Key.GetNeighbors().Where(n => Hexes.Contains(n) &&
                // !TryPathAspect.ContainsKey(n)).OrderBy(x =>
                // x.Distance(TryPathAspect.Last().Key)).First(); if (newHex != null) { var
                // TempAspect = new Dictionary<Hex, List<string>>(); TempAspect.Add(anyHex.Key,
                // anyHex.Value); TempAspect.Add(newHex, anyHex.Value); for (int m = 1; m <
                // TryPathAspect.Count; m++) { TempAspect.Add(TryPathAspect.ElementAt(m).Key,
                // TryPathAspect.ElementAt(m).Value); } TryPathAspect = TempAspect; goto ReTry; } }
                // else if (ReTryCount == 2) { var anyHex = TryPathAspect.Last(); var newHex =
                // anyHex.Key.GetNeighbors() .Where(n => Hexes.Contains(n) &&
                // !TryPathAspect.ContainsKey(n)).OrderBy(x =>
                // x.Distance(TryPathAspect.First().Key)).First(); if (newHex != null) { var
                // TempAspect = new Dictionary<Hex, List<string>>();

                // for (int m = 0; m < TryPathAspect.Count - 1; m++) {
                // TempAspect.Add(TryPathAspect.ElementAt(m).Key, TryPathAspect.ElementAt(m).Value);
                // } TempAspect.Add(newHex, anyHex.Value); TempAspect.Add(anyHex.Key, anyHex.Value);
                // TryPathAspect = TempAspect; goto ReTry; } } else if (ReTryCount == 3) { } } else
                // { foreach (var item in SubPossible.First()) { WaitDelPoint.Add(item.Key); }
                // Solvess.Add(SubPossible.First()); } } foreach (var item in Solvess) { foreach
                // (var item2 in item) { if (TargeItem.ContainsKey(item2.Key)) continue;

                // if (Solver.ContainsKey(item2.Key) && Solver[item2.Key] != item2.Value) {
                // Solver.Clear(); goto End; } else if (!Solver.ContainsKey(item2.Key)) {
                // Solver.Add(item2.Key, item2.Value); } } } }
                if (Solver.Count != 0)
                {
                    StringBuilder RetString = new StringBuilder();
                    foreach (var item in Solver)
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
                    if (Solver.Count == 0)
                        Program.Solves2(Notes);
                }
            }
            ShutdownResetEvent.SetResult(0);
            return await ShutdownResetEvent.Task.ConfigureAwait(false);
        }

        internal static readonly TaskCompletionSource<byte> ShutdownResetEvent = new TaskCompletionSource<byte>();

        private static Dictionary<Hex, string> GetPossibleSolve(Dictionary<Hex, string>[] OriSolver)
        {
            var Success = new List<(Dictionary<Hex, string>, Dictionary<string, int>)>();
            var Fail = new List<(Dictionary<Hex, string>, Dictionary<string, int>)>();
            foreach (var item in OriSolver)
            {
                List<((string, string), Dictionary<Hex, string>)> TempWannaPath = new();
                var First = item.First();
                var Second = item.Last();
                for (int m = 0; m < TargeItem.Count; m++)
                {
                    var CurrectTag = TargeItem.ElementAt(m);
                    if (CurrectTag.Key == First.Key || CurrectTag.Key == Second.Key) continue;
                    var TryPathAspect = new Dictionary<Hex, List<string>>();

                    var ShortestPath = FindShortestPathToChain(CurrectTag.Key, item.Keys.ToList(), Hexes);

                    foreach (var path in ShortestPath)
                    {
                        TryPathAspect.Add(path, new List<string>());
                    }
                    TryPathAspect.First().Value.Add(CurrectTag.Value);

                    var OrderList2 = item[TryPathAspect.Last().Key];

                    TryPathAspect.Last().Value.Add(OrderList2);

                    var GetSolve = GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                    if (GetSolve.Length == 0)
                    {
                        goto End;
                    }
                    TempWannaPath.Add(((CurrectTag.Key.ToString(), ShortestPath.Last().ToString()), GetSolve.First()));
                }
                foreach (var SaveToSolves in TempWannaPath)
                {
                    foreach (var item2 in SaveToSolves.Item2)
                    {
                        if (!item.ContainsKey(item2.Key))
                            item.Add(item2.Key, item2.Value);
                        else if (item[item2.Key] != item2.Value)
                            goto End;
                    }
                }
                foreach (var item2 in TargeItem)
                {
                    item.Remove(item2.Key);
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
                var LastCheck = true;
                foreach (var item2 in 元素耗费)
                {
                    var ApectAspectCheck = NeedCombine(item2.Key, item2.Value, RetNeed);
                    LastCheck = NeedCombine(item2.Key, item2.Value, RetNeed);
                }
                bool NeedCombine(string WaitCombineASpect, int Count, Dictionary<string, int> SaveCount)
                {
                    bool RetCheck = true;
                    if (UserAspect.ContainsKey(WaitCombineASpect))//值存在
                    {
                        if (UserAspect[WaitCombineASpect] - Count < 0)//但是值不足
                        {
                            if (!string.IsNullOrEmpty(AllAspect[WaitCombineASpect].Item1))//并不是基础元素
                            {
                                RetCheck = NeedCombine(AllAspect[WaitCombineASpect].Item1, Math.Abs(UserAspect[WaitCombineASpect] - Count), SaveCount);
                                RetCheck = NeedCombine(AllAspect[WaitCombineASpect].Item2, Math.Abs(UserAspect[WaitCombineASpect] - Count), SaveCount);
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
                        if (!string.IsNullOrEmpty(AllAspect[WaitCombineASpect].Item1))
                        {
                            if (RetCheck)
                                RetCheck = NeedCombine(AllAspect[WaitCombineASpect].Item1, Count, SaveCount);
                            else
                                NeedCombine(AllAspect[WaitCombineASpect].Item1, Count, SaveCount);
                            if (RetCheck)
                                RetCheck = NeedCombine(AllAspect[WaitCombineASpect].Item2, Count, SaveCount);
                            else
                                NeedCombine(AllAspect[WaitCombineASpect].Item1, Count, SaveCount);
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
                if (LastCheck)
                {
                    return item;
                }
                else
                {
                    Fail.Add((item, RetNeed));
                }
            End: continue;
                {
                    //if (LastCheck)
                    //    Success.Add((item, RetNeed));
                    //else
                    //    Fail.Add((item, RetNeed));
                }
            }
            return new Dictionary<Hex, string>();
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

        private static IEnumerable<Dictionary<Hex, string>> GetPossibleMoves(Dictionary<Hex, List<string>> OriList, Dictionary<Hex, string> RetSaveList, int CurretCount)
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
                foreach (var result in GetPossibleMoves(OriList, SaveList, CurretCount))
                {
                    yield return result;
                }
            }
        }
    }
}
