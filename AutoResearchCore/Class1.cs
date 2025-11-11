using System;
using System.Collections;
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

        // 修正后的笛卡尔坐标转换
        public (double x, double y) ToCartesian()
        {
            // 正确的轴向到笛卡尔坐标转换
            double x = Math.Sqrt(3) * q + Math.Sqrt(3) / 2 * r;
            double y = 0.0 * q + 3.0 / 2 * r;
            return (x, y);
        }

        // 计算角度（0-360度）
        public double Angle()
        {
            var (x, y) = ToCartesian();
            double angle = Math.Atan2(y, x) * (180 / Math.PI);
            return angle < 0 ? angle + 360 : angle;
        }

        public double ClockwiseAngle()
        {
            var (x, y) = ToCartesian();
            // Atan2返回的是逆时针角度，我们转换为顺时针
            double angle = -Math.Atan2(y, x) * (180 / Math.PI);
            return angle < 0 ? angle + 360 : angle;
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

                while (true)
                {
                    Hex center = new Hex(0, 0);

                    // 按角度排序所有点
                    var sortedPoints = TargeItem.OrderBy(h => h.Key.Angle()).ToList();

                    // 我们需要找到最接近45°, 135°, 225°, 315°的点
                    var targetAngles = new double[] { 60, 250 };
                    Dictionary<Hex, string> result = new();

                    foreach (var targetAngle in targetAngles)
                    {
                        // 找到最接近目标角度的点
                        var closest = sortedPoints.OrderBy(h => Math.Abs(h.Key.Angle() - targetAngle)).First();
                        result.Add(closest.Key, closest.Value);
                    }

                    List<Hex> GetHexRing(Hex center, int radius)
                    {
                        var results = new List<Hex>();

                        if (radius == 0)
                        {
                            results.Add(center);
                            return results;
                        }

                        // 六个方向（顺时针）
                        var directions = new (int dq, int dr)[] { (1, 0), (1, -1), (0, -1), (-1, 0), (-1, 1), (0, 1) };
                        // 从第一个方向的起点出发
                        int q = center.q + directions[4].dq * radius;
                        int r = center.r + directions[4].dr * radius;
                        Hex hex = new Hex(q, r);

                        for (int i = 0; i < 6; i++)
                        {
                            for (int step = 0; step < radius; step++)
                            {
                                results.Add(hex);
                                q += directions[i].dq;
                                r += directions[i].dr;
                                hex = new Hex(q, r);
                            }
                        }

                        return results;
                    }
                    TargeItem = TargeItem.OrderBy(kvp => kvp.Key.ClockwiseAngle()).ToDictionary();

                    var topHex = result.First();
                    var bottomHex = result.Last();
                    var ShortestPathRoot = FindShortestPathToChain(topHex.Key, [bottomHex.Key], Hexes);
                    var TryPathAspect = new Dictionary<Hex, List<string>>();
                    foreach (var item in ShortestPathRoot)
                    {
                        TryPathAspect.Add(item, []);
                    }
                    TryPathAspect[topHex.Key].Add(topHex.Value);
                    TryPathAspect[bottomHex.Key].Add(bottomHex.Value);

                    Dictionary<Hex, string>? SolverM = GetPossibleSolve(GetPossibleMoves(TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray());
                    if (SolverM.Count == 0)
                    {
                        var linksecond = GetHexRing(center, 2);
                        var set = new HashSet<Hex>(linksecond);
                        var Check = TargeItem.Keys.All(x => set.Contains(x));
                        if (TargeItem.Keys.All(set.Contains))
                        {
                            var linkfirst = GetHexRing(center, 1);
                            Dictionary<(Hex, Hex), Dictionary<Hex, string>[]> _SolverS = new();
                            Dictionary<Hex, List<string>> TrySolvers = new();

                            for (int j = 0; j < TargeItem.Count; j++)
                            {
                                KeyValuePair<Hex, string> CurrectItem = TargeItem.ElementAt(j);
                                var Neighbor = linkfirst.OrderBy(x => x.Distance(CurrectItem.Key)).Intersect(Hexes).ToArray();
                                foreach (var item in Neighbor)
                                {
                                    if (TrySolvers.ContainsKey(item))
                                    {
                                        continue;
                                    }
                                    TrySolvers.Add(item, AspectMap[CurrectItem.Value]);
                                    break;
                                }
                            }
                            var GetAllCombinations = GetPossibleMoves2(TrySolvers, new Dictionary<Hex, string>(), new Dictionary<Hex, Hex>(), 0).OrderBy(x => x.Fail.Count).First();

                            if (GetAllCombinations.Fail.Count == 2)
                            {
                                var item = GetAllCombinations.Fail.First();
                                var CFirst = item.Key;
                                var CLast = item.Value;
                                var CFirstAspect = AspectMap[GetAllCombinations.Sucess[CFirst]];
                                var CLastAspect = AspectMap[GetAllCombinations.Sucess[CLast]];
                                var CIntersect = CFirstAspect.Intersect(CLastAspect).ToList();
                                if (CIntersect.Count == 0)
                                {
                                    var CFirstlinksecond = CFirst.GetNeighbors().Intersect(linkfirst).Except([CLast]).FirstOrDefault();
                                    if (CFirstlinksecond != null)
                                    {
                                        CFirst = CFirstlinksecond;
                                        CFirstAspect = AspectMap[GetAllCombinations.Sucess[CFirst]];
                                        CIntersect = CFirstAspect.Intersect(CLastAspect).ToList();
                                        if (CIntersect.Count != 0)
                                        {
                                            GetAllCombinations.Sucess.Add(center, CIntersect.First());
                                        }
                                    }
                                }
                            }

                            for (int j = 0, m = 1; j < TargeItem.Count; j++, m++)
                            {
                                if (m == TargeItem.Count) m = 0;
                                {
                                    var First = TargeItem.ElementAt(j);
                                    var Last = TargeItem.ElementAt(m);
                                    var FirstIndex = linksecond.IndexOf(First.Key);
                                    if (FirstIndex == -1) continue;
                                    var LastIndex = linksecond.IndexOf(Last.Key);
                                    if (LastIndex == -1) continue;
                                    var _TryPathAspect = new Dictionary<Hex, List<string>>();
                                    var Currect = FirstIndex;
                                    var LinkCheck = true;
                                    do
                                    {
                                        try
                                        {
                                            if (Currect == FirstIndex)
                                            {
                                                _TryPathAspect.Add(First.Key, [First.Value]);
                                                continue;
                                            }
                                            else if (Currect == LastIndex)
                                            {
                                                _TryPathAspect.Add(Last.Key, [Last.Value]);
                                                break;
                                            }
                                            var CurrectPoint = linksecond[Currect];
                                            if (Hexes.Contains(CurrectPoint))
                                            {
                                                _TryPathAspect.Add(CurrectPoint, []);
                                            }
                                            else
                                            {
                                                LinkCheck = false;
                                                break;
                                            }
                                        }
                                        catch (Exception)
                                        {
                                        }
                                        finally
                                        {
                                            Currect += 1;
                                            if (Currect == linksecond.Count)
                                                Currect = 0;
                                        }
                                    } while (true);
                                    if (LinkCheck)
                                    {
                                        var _Solver = GetPossibleMoves(_TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                                        if (_Solver.Length == 0)
                                        {
                                            var FirstPointNeighbor = _TryPathAspect.First().Key.GetNeighbors().ToArray();
                                            var SecondPointNeighbor = _TryPathAspect.Last().Key.GetNeighbors().ToArray();
                                            var GetCrossPoint = FirstPointNeighbor.Intersect(SecondPointNeighbor).Except(linkfirst).ToArray();
                                            if (GetCrossPoint.Length != 0)
                                            {
                                                foreach (var CrossPoint in GetCrossPoint)
                                                {
                                                    if (Hexes.Contains(CrossPoint))
                                                    {
                                                        _TryPathAspect = new Dictionary<Hex, List<string>>()
                                                    {
                                                        { First.Key,[First.Value]},
                                                        { CrossPoint,[]},
                                                        { Last.Key,[Last.Value]},
                                                    };
                                                        _Solver = GetPossibleMoves(_TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                                                        _SolverS.Add((First.Key, Last.Key), _Solver);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                var SubFirstPoint = FirstPointNeighbor.Intersect(linkfirst).Intersect(Hexes).FirstOrDefault();
                                                var SubSecondPoint = SecondPointNeighbor.Intersect(linkfirst).Intersect(Hexes).FirstOrDefault();
                                                if (SubFirstPoint != null && SubSecondPoint != null)
                                                {
                                                    _TryPathAspect = new Dictionary<Hex, List<string>>()
                                                    {
                                                        { First.Key,[First.Value]},
                                                        { SubFirstPoint,[]},
                                                        { SubSecondPoint,[]},
                                                        { Last.Key,[Last.Value]},
                                                    };
                                                    _Solver = GetPossibleMoves(_TryPathAspect, new Dictionary<Hex, string>(), 0).ToArray();
                                                    _SolverS.Add((First.Key, Last.Key), _Solver);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            _SolverS.Add((First.Key, Last.Key), _Solver);
                                        }
                                    }
                                }
                            }

                            foreach (var item in _SolverS)
                            {
                                if (item.Value.Length != 0)
                                {
                                    foreach (var item2 in item.Value.First())
                                    {
                                        if (TargeItem.ContainsKey(item2.Key)) continue;
                                        if (!SolverM.ContainsKey(item2.Key))
                                        {
                                            SolverM.Add(item2.Key, item2.Value);
                                        }
                                    }
                                }
                            }
                            foreach (var item in GetAllCombinations.Sucess)
                            {
                                if (TargeItem.ContainsKey(item.Key)) continue;
                                if (!SolverM.ContainsKey(item.Key))
                                {
                                    SolverM.Add(item.Key, item.Value);
                                }
                            }
                            //最终连线确认以及连接//首先确认所有Target是否都能跑通

                            foreach (var item in TargeItem)
                            {
                                var TargeNei = item.Key.GetNeighbors();
                                var TargeAspect = AspectMap[item.Value];
                                var FindCheck = false;
                                foreach (var TargeNeiIntersect in TargeNei.Intersect(SolverM.Keys).ToList())
                                {
                                    if (TargeAspect.Contains(SolverM[TargeNeiIntersect]))
                                    {
                                        FindCheck = true;
                                        break;
                                    }
                                }
                                if (!FindCheck)
                                {
                                    //找寻四周是否有空白点
                                    var SpacePoints = Hexes.Except(SolverM.Keys).Except(TargeItem.Keys).Intersect(TargeNei).ToArray().FirstOrDefault();
                                    if (SpacePoints != null)
                                    {
                                        //确认这个点附近的所有点
                                        var SpacePointsNeighbors = SpacePoints.GetNeighbors().Intersect(SolverM.Keys).ToArray();
                                        foreach (var item2 in SpacePointsNeighbors)//获得这两个点中的一个，是否与该点有交集
                                        {
                                            var AspectMapTarge = AspectMap[SolverM[item2]].Intersect(TargeAspect).ToArray();
                                            if (AspectMapTarge.Length != 0)
                                            {
                                                SolverM.Add(SpacePoints, AspectMapTarge.First());
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (SolverM.Count != 0)
                    {
                        StringBuilder RetString = new StringBuilder();
                        foreach (var item in SolverM)
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

                    break;
                }
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
                        //var WaitDelPoint = new List<Hex>();
                        //var Solvess = new List<Dictionary<Hex, string>>();
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
                        Dictionary<Hex, Dictionary<Hex, string>[]> HexSolves = new();
                        for (int i = 0, j = 1; i < sortedTargeItem.Count; i++, j++)
                        {
                            if (j == sortedTargeItem.Count - 1 && HexSolves.Count == sortedTargeItem.Count - 1)
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
                                //foreach (var item in SubPossible.First())
                                //{
                                //    WaitDelPoint.Add(item.Key);
                                //}
                                //Solvess.Add(SubPossible.First());
                                if (HexSolves.ContainsKey(First.Key))
                                {
                                    HexSolves[First.Key] = SubPossible;
                                }
                                else
                                {
                                    HexSolves.Add(First.Key, SubPossible);
                                }
                            }
                        }
                        if (HexSolves.Count >= sortedTargeItem.Count - 1)
                        {
                            Dictionary<Hex, string> Solver = new();
                            var All = GetSolvesMoves(HexSolves, new Dictionary<Hex, string>(), 0).ToArray();
                            foreach (var Solvess in GetSolvesMoves(HexSolves, new Dictionary<Hex, string>(), 0))
                            {
                                foreach (var item2 in Solvess)
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
                                break;
                            End: continue;
                            }

                            //foreach (var item in Solvess)
                            //{
                            //    foreach (var item2 in item)
                            //    {
                            //        if (TargeItem.ContainsKey(item2.Key)) continue;

                            //        if (Solver.ContainsKey(item2.Key) && Solver[item2.Key] != item2.Value)
                            //        {
                            //            Solver.Clear();
                            //            goto End;
                            //        }
                            //        else if (!Solver.ContainsKey(item2.Key))
                            //        {
                            //            Solver.Add(item2.Key, item2.Value);
                            //        }
                            //    }
                            //}
                            if (Solver.Count != 0)
                                Solves.Add(Solver);
                        }
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
                #region
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
                #endregion
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
                    //Dictionary<string, int> AspectDic = new();
                    //if (File.Exists("AspectTotal.txt"))
                    //{
                    //    var ReadAllAspect = File.ReadAllLines("AspectTotal.txt");
                    //    foreach (var item in ReadAllAspect)
                    //    {
                    //        var SP = item.Split('|');
                    //        if (SP.Length == 2)
                    //        {
                    //            if (int.TryParse(SP[1],out int Num))
                    //            {
                    //                AspectDic.Add(SP[0],Num);
                    //            }
                    //        }
                    //    }
                    //}
                    //{
                    //    foreach (var item in Solver)
                    //    {
                    //        if(AspectDic.ContainsKey(item.Value))
                    //        {
                    //            AspectDic[item.Value] += 1;
                    //        }
                    //        else
                    //        {
                    //            AspectDic.Add(item.Value,1);
                    //        }
                    //    }
                    //    StringBuilder stringBuilder = new();
                    //    foreach (var item in AspectDic.OrderByDescending(x=>x.Value))
                    //    {
                    //        stringBuilder.AppendLine($"{item.Key}|{item.Value}");
                    //    }
                    //    File.WriteAllText("AspectTotal.txt", stringBuilder.ToString());
                    //}
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
            //var Success = new List<(Dictionary<Hex, string>, Dictionary<string, int>)>();
            //var Fail = new List<(Dictionary<Hex, string>, Dictionary<string, int>)>();
            //var AllTry = new List<(List<Dictionary<Hex, string>> Success, List<Dictionary<Hex, List<string>>> Fail)>();
            foreach (var item in OriSolver)
            {
                List<((string, string), Dictionary<Hex, string>)> TempWannaPath = new();
                var First = item.First();
                var Second = item.Last();

                var Success = new Dictionary<Hex, string>();
                var Fail = new List<Dictionary<Hex, List<string>>>();

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
                        Fail.Add(TryPathAspect);
                        continue;
                        //goto End;
                    }
                    bool CheckSuccess = false;
                    foreach (var SuccessCheck in GetSolve)
                    {
                        var WaitAddSuccess = new Dictionary<Hex, string>();
                        foreach (var SuccessCheck2 in SuccessCheck)
                        {
                            if (Success.ContainsKey(SuccessCheck2.Key))
                            {
                                if (Success[SuccessCheck2.Key] != SuccessCheck2.Value)
                                {
                                    goto CheckFail;
                                }
                            }
                            else
                            {
                                WaitAddSuccess.Add(SuccessCheck2.Key, SuccessCheck2.Value);
                            }
                        }
                        CheckSuccess = true;
                        foreach (var SaveSuccess in WaitAddSuccess)
                        {
                            if (!Success.ContainsKey(SaveSuccess.Key))
                            {
                                Success.Add(SaveSuccess.Key, SaveSuccess.Value);
                            }
                        }
                        TempWannaPath.Add(((CurrectTag.Key.ToString(), ShortestPath.Last().ToString()), WaitAddSuccess));
                        break;
                    CheckFail: continue;
                    }
                    if (!CheckSuccess)
                    {
                        Fail.Add(TryPathAspect);
                    }
                }
                if (Fail.Count != 0)
                {
                    //AllTry.Add((Success, Fail));
                    if (Fail.Count == 1)
                    {
                        var ExistPointList = new Dictionary<Hex, string>();
                        foreach (var ExistPoint in Success)
                        {
                            if (ExistPointList.ContainsKey(ExistPoint.Key))
                            {
                                goto End;
                            }
                            else
                            {
                                ExistPointList.Add(ExistPoint.Key, ExistPoint.Value);
                            }
                        }
                        var TryPathAspect = Fail.First();
                        var anyHex = TryPathAspect.Last();
                        var newHex = anyHex.Key.GetNeighbors()
                        .Where(n => Hexes.Contains(n) && !ExistPointList.ContainsKey(n) && !TryPathAspect.ContainsKey(n)).OrderBy(x => x.Distance(TryPathAspect.First().Key)).First();
                        if (newHex != null)
                        {
                            var TempAspect = new Dictionary<Hex, List<string>>();

                            for (int m = 0; m < TryPathAspect.Count - 1; m++)
                            {
                                TempAspect.Add(TryPathAspect.ElementAt(m).Key, TryPathAspect.ElementAt(m).Value);
                            }
                            TempAspect.Add(newHex, anyHex.Value);
                            TempAspect.Add(anyHex.Key, anyHex.Value);
                            var SubPossible = GetPossibleMoves(TempAspect, new Dictionary<Hex, string>(), 0).ToArray();
                            if (SubPossible.Length != 0)
                            {
                                TempWannaPath.Add(((TryPathAspect.First().Key.ToString(), TryPathAspect.Last().ToString()), SubPossible.First()));
                                goto CONNECT;
                            }
                        }
                    }
                    goto End;
                }
            CONNECT:
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
                    //Fail.Add((item, RetNeed));
                }
            End: continue;
                {
                    //if (LastCheck)
                    //    Success.Add((item, RetNeed));
                    //else
                    //    Fail.Add((item, RetNeed));
                }
            }
            //AllTry = AllTry.OrderBy(x => x.Fail.Count).ToList();

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
            Hex center = new Hex(0, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (targetSet.Contains(current))
                {
                    end = current;
                    break;
                }

                //foreach (var neighbor in current.GetNeighbors().Where(n => !visited.Contains(n) && walkableSet.Contains(n)).OrderByDescending(n => n.GetNeighbors().Count(nn => walkableSet.Contains(nn))))
                foreach (var neighbor in current.GetNeighbors().OrderBy(n => n.Distance(center)))
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
                //foreach (var item in AspectMap[RetSaveList[OriList.ElementAt(CurretCount - 1).Key]].OrderByDescending(a => UserAspect.TryGetValue(a, out var c) ? c : 0))
                //// 1. 获取获得当前坐标
                //var outerKey = OriList.ElementAt(CurretCount - 1).Key;

                //// 2. 获得当前坐标所对应的元素
                //var retSaveKey = RetSaveList[outerKey];

                //// 3. 获得元素所对应的下一级元素组合
                //var aspectCollection = AspectMap[retSaveKey];

                //// 4. 根据用户该值的数量加上随机偏移量排序
                //var sortedCollection = aspectCollection.OrderByDescending(a =>
                //{
                //    // 尝试从 UserAspect 字典中获取值，如果不存在则使用 0
                //    double userAspectValue = RetSaveList.ContainsValue(a)?-100: UserAspect.TryGetValue(a, out var c) ? c : 0;

                //    // 生成随机数并加上用户特征值
                //    return userAspectValue + Random.Shared.NextDouble() * 0.5;
                //});
                //foreach (var item in sortedCollection)
                foreach (var item in AspectMap[RetSaveList[OriList.ElementAt(CurretCount - 1).Key]].OrderByDescending(a => (RetSaveList.ContainsValue(a) ? -100 : UserAspect.TryGetValue(a, out var c) ? c : 0) + Random.Shared.NextDouble() * 0.5))
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

        public static IEnumerable<(Dictionary<Hex, string> Sucess, Dictionary<Hex, Hex> Fail)> GetPossibleMoves2(Dictionary<Hex, List<string>> OriList, Dictionary<Hex, string> RetSaveList, Dictionary<Hex, Hex> FailPoint, int CurretCount)
        {
            if (CurretCount >= OriList.Count)
            {
                yield return (RetSaveList, FailPoint);
                yield break;
            }
            var CurrectHex = OriList.ElementAt(CurretCount);

            CurretCount += 1;

            if (RetSaveList.Count == 0)
            {
                foreach (var item in CurrectHex.Value)
                {
                    var SaveList = new Dictionary<Hex, string>(RetSaveList);

                    SaveList.Add(CurrectHex.Key, item);
                    foreach (var result in GetPossibleMoves2(OriList, SaveList, FailPoint, CurretCount))
                    {
                        yield return result;
                    }
                }
            }
            else
            {
                var PreAspect = AspectMap[RetSaveList.Last().Value].OrderBy(a => UserAspect.TryGetValue(a, out var c) ? c : 0).ToList();
                var CurrectIntersect = CurrectHex.Value.Intersect(PreAspect).ToList();
                if (CurrectIntersect.Count != 0)
                {
                    foreach (var item in CurrectIntersect)
                    {
                        var SaveList = new Dictionary<Hex, string>(RetSaveList);
                        SaveList.Add(CurrectHex.Key, item);
                        foreach (var result in GetPossibleMoves2(OriList, SaveList, FailPoint, CurretCount))
                        {
                            yield return result;
                        }
                    }
                }
                else
                {
                    var WaitAddFailPoint = new Dictionary<Hex, Hex>(FailPoint);
                    WaitAddFailPoint.Add(RetSaveList.Last().Key, CurrectHex.Key);
                    foreach (var item in CurrectHex.Value)
                    {
                        var SaveList = new Dictionary<Hex, string>(RetSaveList);
                        SaveList.Add(CurrectHex.Key, item);
                        foreach (var result in GetPossibleMoves2(OriList, SaveList, WaitAddFailPoint, CurretCount))
                        {
                            yield return result;
                        }
                    }
                }
            }
        }

        private static IEnumerable<Dictionary<Hex, string>> GetSolvesMoves(Dictionary<Hex, Dictionary<Hex, string>[]> OriList, Dictionary<Hex, string> RetSaveList, int CurretCount)
        {
            if (CurretCount >= OriList.Count)
            {
                yield return new Dictionary<Hex, string>(RetSaveList);
                yield break;
            }

            var GetCurrect = OriList.ElementAt(CurretCount).Value;

            CurretCount += 1;

            foreach (var item in GetCurrect)
            {
                var SaveList = new Dictionary<Hex, string>();
                foreach (var preitem in RetSaveList)
                {
                    SaveList.Add(preitem.Key, preitem.Value);
                }
                foreach (var WaitAdd in item)
                {
                    if (!SaveList.ContainsKey(WaitAdd.Key))
                        SaveList.Add(WaitAdd.Key, WaitAdd.Value);
                    else if (SaveList[WaitAdd.Key] != WaitAdd.Value)
                    {
                        goto End;
                    }
                }
                foreach (var result in GetSolvesMoves(OriList, SaveList, CurretCount))
                {
                    yield return result;
                }
            End: continue;
            }
        }
    }
}
