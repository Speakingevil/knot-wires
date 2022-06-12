using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using KModkit;

public class KnotWiresScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public KMBombInfo info;
    public KMColorblindMode colorblindMode;
    public GameObject modalign;
    public GameObject[] knots;
    public GameObject[] components;
    public GameObject[] batteries;
    public Renderer[] wires;
    public Renderer[] buttoninds;
    public Renderer[] battinds;
    public Renderer[] leds;
    public Renderer screen;
    public KMSelectable button;
    public TextMesh[] displays;
    public TextMesh[] cbtexts;
    public Material[] mats;
    public Material[] wiremats;
    public Material[] lits;
    public Material off;

    private static int[] nodup = new int[10] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }; 
    private bool cb;
    private int[] rotations = new int[5];
    private int moduletype;
    private int battype;
    private string modsn;
    private int[] colints = new int[6];
    private int buttonind;
    private int[] screenints = new int[2] { -1, -1 };
    private int dispnum;
    private List<int> answer = new List<int> { };
    private List<int> submissions = new List<int> { };
    private bool relseq;
    //Type Specific
    private bool[][] indconds = new bool[3][] { new bool[99], new bool[99], new bool[99] };
    private int[] startnum = new int[2];
    private char[,] maze = new char[15,15];
    private int[] pos = new int[6];
    private int[] dir = new int[7] {-1,0,0,0,0,0,0};
    private readonly string[] morse = new string[36] { "-----", ".----", "..---", "...--", "....-", ".....", "-....", "--...", "---..", "----.", ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".----", "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-", "..-", "...-", ".--", "-..-", "-.--", "--.." };
    private string[] morseflash = new string[3];
    private List<int> morsesub = new List<int> { };
    private IEnumerator[] ms = new IEnumerator[3];
    private bool[] digitone;
    private int[] tapdiff = new int[8];
    private int[][] colencoding = new int[5][];
    private int[,] factors = new int[3, 9];
    private long product = 1;
    private List<int> dpairs = new List<int> { };
    private List<int> relnums;
    private int[] crelnums;
    private int[] sounds = new int[10];
    private bool[] numalts = new bool[8];
    private int[] pdivs = new int[3];
    private int[][] bigdig = new int[2][] { new int[5], new int[3]};
    private List<int> digitorder = new List<int> { };
    private List<int>[] initnums = new List<int>[2] { new List<int> { }, new List<int> { } };
    private bool[][] shownum = new bool[2][] { new bool[7], new bool[7]};
    private int inittime;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        cb = colorblindMode.ColorblindModeActive;
        rotations = new int[4] { 0, 1, 2, 3 }.Shuffle().Concat(new int[1] { Random.Range(0, 4) }).ToArray();
        battype = Random.Range(0, 4);        
        buttonind = Random.Range(0, 4);
        for (int i = 0; i < 4; i++)
            components[i].transform.localEulerAngles = new Vector3(0, 90 * rotations[i], 0);
        modalign.transform.localEulerAngles = new Vector3(0, -90 * rotations[4], 0);        
        module.OnActivate = Activate;
    }

    private void Activate()
    { 
        if (moduleIDCounter == moduleID)
            nodup.Shuffle();
        if (moduleIDCounter - moduleID < 10)
            moduletype = nodup[moduleIDCounter - moduleID];
        else
            moduletype = Random.Range(0, 10);
        if (moduletype == 7)
            colints = new int[] { 0, 1, 2, 3, 4, 5, 6 }.Shuffle().Take(6).ToArray();
        else
            for (int i = 0; i < 6; i++)
                colints[i] = Random.Range(0, 7);
        moduletype = 9;
        int t = Random.Range(0, 2);
        for (int i = 0; i < 20; i++)
            if (i / 2 != moduletype || i % 2 == t)
                knots[i].SetActive(false);
            else
                for (int j = 0; j < 8; j++)
                    wires[8 * i + j].material = wiremats[colints[5]];
        for (int i = 0; i < 4; i++)
            if (i != battype)
                batteries[i].SetActive(false);
            else
                for (int j = 0; j < 2; j++)
                    battinds[2 * i + j].material = mats[colints[4]];
        foreach (Renderer b in buttoninds)
            b.material = mats[colints[3]];
        displays[1].text = new string[4] { "PRESS", "PUSH", "HOLD", "ABORT" }[buttonind];
        for (int i = 0; i < 3; i++)
            leds[i].material = lits[colints[i]];
        if (cb)
        {
            displays[0].fontSize = 100;
            for (int i = 0; i < 6; i++)
                cbtexts[i].text = "RGBCMYW"[colints[i]].ToString();
        }
        modsn = new string[10] { "A", "C", "O", "R", "S", "T", "V", "X", "Y", "Z" }[moduletype];
        modsn += "-";
        for (int i = 0; i < 3; i++)
        {
            int r = rotations[i + 1] - rotations[0] + (rotations[i + 1] > rotations[0] ? 0 : 4) - 1;
            modsn += new string[9] { "D", "I", "P", "L", "K", "F", "B", "H", "N" }[3 * i + r];
        }
        switch (moduletype)
        {
            case 1:
            case 3:
                modsn += "WEM";
                break;
            case 5:
            case 9:
                if (rotations[3] % 2 == 1)
                    modsn += "WEM";
                else
                    for (int i = 0; i < 3; i++)
                        modsn += Mathf.Abs(rotations[i] - rotations[3]) == 2 ? "UQJ"[i] : "WEM"[i];
                break;
            case 4:
                for (int i = 0; i < 3; i++)
                    modsn += rotations[i] + rotations[3] == 3 ? "UQJ"[i] : "WEM"[i];
                break;
            case 6:
                switch (rotations[3])
                {
                    case 0:
                        modsn += "WEM";
                        break;
                    case 2:
                        for (int i = 0; i < 3; i++)
                            modsn += rotations[i] == 0 ? "WEM"[i] : "UQJ"[i];
                        break;
                    default:
                        for (int i = 0; i < 3; i++)
                            modsn += rotations[i] == 2 ? "UQJ"[i] : "WEM"[i];
                        break;
                }
                break;
            default:
                for(int i = 0; i < 3; i++)
                    modsn += Mathf.Abs(rotations[i] - rotations[3]) == 2 ? "UQJ"[i] : "WEM"[i];
                break;
        }
        Debug.LogFormat("[Knot Wires #{0}] The button is {1}, is labelled \"{2}\" and faces {3}.", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white"} [colints[3]], new string[] { "PRESS", "PUSH", "HOLD", "ABORT"}[buttonind], new string[] { "south", "west", "north", "east" }[(rotations[0] - rotations[4] + 4) % 4]);
        Debug.LogFormat("[Knot Wires #{0}] The LEDs are {1}, {2}, and {3} and face {4}.", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[0]], new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[1]], new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[2]], new string[] { "south", "west", "north", "east" }[(rotations[1] - rotations[4] + 4) % 4]);
        Debug.LogFormat("[Knot Wires #{0}] The screen faces {1}.", moduleID, new string[] { "south", "west", "north", "east" }[(rotations[2] - rotations[4] + 4) % 4]);
        Debug.LogFormat("[Knot Wires #{0}] The {1} batter{2} {3} and face{4} {5}.", moduleID, new string[] { "AA", "AAA", "D", "9V" }[battype], new string[] { "ies are", "y is"}[battype / 2], new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[4]], new string[] { "", "s" }[battype / 2], new string[] { "south", "west", "north", "east" }[(rotations[3] - rotations[4] + 4) % 4]);
        Debug.LogFormat("[Knot Wires #{0}] The wires are {1}.", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[5]]);
        Debug.LogFormat("[Knot Wires #{0}] The module type is: {1}.", moduleID, modsn);
        switch (moduletype)
        {
            case 0:
                int[] holdtimes = new int[2] { -1, -1 };
                if (colints[3] == 6)
                    holdtimes[0] = 0;
                else if (!colints.Take(3).Any(x => colints.Skip(3).Contains(x)))
                    holdtimes[0] = 1;
                else if (colints[4] == 0)
                    holdtimes[0] = 2;
                else if (!colints.Contains(5))
                    holdtimes[0] = 3;
                else if (new string[4] { "PRESS", "PUSH", "HOLD", "ABORT" }[buttonind].Any(x => modsn.Skip(2).Contains(x)))
                    holdtimes[0] = 4;
                else if (battype == 2)
                    holdtimes[0] = 5;
                else if (colints.Take(3).Distinct().Count() < 3)
                    holdtimes[0] = 6;
                else if (buttonind == 3)
                    holdtimes[0] = 7;
                else if (colints.Contains(2))
                    holdtimes[0] = 8;
                else
                    holdtimes[0] = 9;
                if (colints[3] == colints[4])
                    holdtimes[1] = 1;
                else if (colints.Where(x => x == 1).Count() > 1)
                    holdtimes[1] = 2;
                else if (colints.Where((x, i) => i > 2).All(x => colints.Where((y, i) => i < 3).Contains(x)))
                    holdtimes[1] = 3;
                else if (colints[1] == 3)
                    holdtimes[1] = 4;
                else if ((rotations[2] - rotations[4] + 4) % 4 == 2)
                    holdtimes[1] = 5;
                else if (!colints.Where((x, i) => i != 4).Contains(colints[4]))
                    holdtimes[1] = 6;
                else if (new string[4] { "PRESS", "PUSH", "HOLD", "ABORT" }[buttonind].Any(x => info.GetSerialNumberLetters().Contains(x)))
                    holdtimes[1] = 7;
                else if (colints.Distinct().Count() == 4)
                    holdtimes[1] = 8;
                else
                    holdtimes[1] = 9;
                answer = holdtimes.ToList();
                Debug.LogFormat("[Knot Wires #{0}] Hold at {1}. Release at {2}.", moduleID, holdtimes[0], holdtimes[1]);
                break;
            case 1:
                switch (colints[0])
                {
                    case 0:
                        for (int i = 1; i < 70; i++)
                            indconds[0][i - 1] = true;
                        break;
                    case 1:
                        for (int i = 1; i < 100; i++)
                            if (i % 3 != 1)
                                indconds[0][i - 1] = true;
                        break;
                    case 2:
                        for (int i = 1; i < 100; i++)
                            if ((i % 10) + (i / 10) < 13)
                                indconds[0][i - 1] = true;
                        break;
                    case 3:
                        for (int i = 1; i < 100; i++)
                            if (Mathf.Abs((i % 10) - (i / 10)) > 1)
                                indconds[0][i - 1] = true;
                        break;
                    case 4:
                        for (int i = 1; i < 100; i++)
                                indconds[0][i - 1] = true;
                        break;
                    case 5:
                        for (int i = 1; i < 100; i++)
                            if ((i % 2) + ((i / 10) % 2) < 2)
                                indconds[0][i - 1] = true;
                        break;
                    default:
                        for (int i = 1; i < 100; i++)
                            if (!((i % 10) == 3 || (i % 10) == 6 || (i / 10) == 3 || (i / 10) == 6))
                                indconds[0][i - 1] = true;
                        break;
                }
                switch (colints[1])
                {
                    case 0:
                        for (int i = 1; i < 100; i++)
                            if (i % 7 != 1 && i % 7 != 6)
                                indconds[1][i - 1] = true;
                        break;
                    case 1:
                        for (int i = 1; i < 100; i++)
                            if (((i % 10) + (i / 10)) % 4 != 0)
                                indconds[1][i - 1] = true;
                        break;
                    case 2:
                        for (int i = 1; i < 100; i++)
                            if (Mathf.Abs((i % 10) - (i / 10)) % 4 != 0)
                                indconds[1][i - 1] = true;
                        break;
                    case 3:
                        for (int i = 1; i < 100; i++)
                            if (i % 7 != 2 && i % 7 != 5)
                                indconds[1][i - 1] = true;
                        break;
                    case 4:
                        for (int i = 1; i < 100; i++)
                            if (((i % 10) + (i / 10)) % 4 != 2)
                                indconds[1][i - 1] = true;
                        break;
                    case 5:
                        for (int i = 1; i < 100; i++)
                            if (Mathf.Abs((i % 10) - (i / 10)) % 4 != 2)
                                indconds[1][i - 1] = true;
                        break;
                    default:
                        for (int i = 1; i < 100; i++)
                            if ((i % 10) + (i / 10) < 8 || (i % 10) + (i / 10) > 10)
                                indconds[1][i - 1] = true;
                        break;
                }
                switch (colints[2])
                {
                    case 0:
                        for (int i = 30; i < 100; i++)
                            indconds[2][i - 1] = true;
                        break;
                    case 1:
                        for (int i = 1; i < 100; i++)
                            if (i % 3 != 2)
                                indconds[2][i - 1] = true;
                        break;
                    case 2:
                        for (int i = 1; i < 100; i++)
                            if ((i % 10) + (i / 10) > 5)
                                indconds[2][i - 1] = true;
                        break;
                    case 3:
                        for (int i = 1; i < 100; i++)
                            if (Mathf.Abs((i % 10) - (i / 10)) < 5)
                                indconds[2][i - 1] = true;
                        break;
                    case 4:
                        for (int i = 1; i < 100; i++)
                            if ((i % 10) < 7)
                                indconds[2][i - 1] = true;
                        break;
                    case 5:
                        for (int i = 1; i < 100; i++)
                            if ((i % 2) + ((i / 10) % 2) > 0)
                                indconds[2][i - 1] = true;
                        break;
                    default:
                        for (int i = 1; i < 100; i++)
                            if (!((i % 10) != 4 && (i % 10) != 5 && (i / 10) != 4 && (i / 10) != 5))
                                indconds[2][i - 1] = true;
                        break;
                }
                for (int i = 0; i < 99; i++)
                    if (indconds[0][i] && indconds[1][i] && indconds[2][i])
                        answer.Add(i + 1);
                Debug.LogFormat("[Knot Wires #{0}] The numbers that satisfy all three conditions are: {1}", moduleID, string.Join(", ", answer.Select(i => i.ToString()).ToArray()));
                break;
            case 2:
                startnum = new int[2] { Random.Range(1, 100), Random.Range(0, 6) };
                answer.Add(startnum[0]);
                string[] truth = new string[4] { "\u00d8", "\u00d8", "\u00d8", "\u00d8"};
                Debug.LogFormat("[Knot Wires #{0}] The starting value is {1}", moduleID, startnum[0].ToString());
                Debug.LogFormat("[Knot Wires #{0}] List {1} is used to modify the starting value", moduleID, "RGBCMY"[startnum[1]]);
                switch (startnum[1])
                {
                    case 0:
                        if (colints[5] < 3) { truth[0] = "T"; answer.Add(answer[0] + 10); } else { truth[0] = "F"; answer.Add(answer[0] - 10); }
                        if (answer[1] > 50) { truth[1] = "T"; answer.Add(answer[1] - 50); } else { truth[1] = "F"; answer.Add(answer[1]); }
                        if (colints.Where(x => x > 2 && x != 6).Count() > 2) { truth[2] = "T"; answer.Add(50 - answer[2]); } else { truth[2] = "F"; answer.Add(answer[2] * 2); }
                        answer.Add(answer[3] + ((answer[3] - 1) % 9) + 1);
                        break;
                    case 1:
                        if ((rotations[0] - rotations[4] + 4) % 2 == 1) { truth[0] = "T"; answer.Add(answer[0] + info.GetSerialNumberNumbers().First()); } else { truth[0] = "F"; answer.Add(answer[0] + info.GetSerialNumberNumbers().Last()); }
                        if (answer[1] % 2 == 0) { truth[1] = "T"; answer.Add(answer[1] / 2); } else { truth[1] = "F"; answer.Add(answer[1] - 1); }
                        answer.Add(answer[2] - (Mathf.Abs(answer[2]) % 10) - ((Mathf.Abs(answer[2]) / 10) % 10) - (Mathf.Abs(answer[2]) / 100));
                        if (colints.Where((x, i) => i < 3).Distinct().Count() < 3) { truth[3] = "T"; answer.Add(answer[3] - ((Mathf.Abs(answer[3]) / 10) % 10)); } else { truth[3] = "F"; answer.Add(answer[3] + (Mathf.Abs(answer[3]) % 10)); }
                        break;
                    case 2:
                        if (answer[0] % 10 > answer[0] / 10) { truth[0] = "T"; answer.Add((answer[0] % 10) * 10 + (answer[0] / 10)); } else { truth[0] = "F"; answer.Add(answer[0]); }
                        if (buttonind < 2) { truth[1] = "T"; answer.Add(answer[1] + (info.GetPortCount() * 3) ); } else { truth[1] = "F"; answer.Add(answer[1] + (info.GetBatteryCount() * 3)); }
                        answer.Add(Mathf.Abs(answer[2] - answer[0]));
                        if (answer[3] < info.GetModuleNames().Count()) { truth[3] = "T"; answer.Add(answer[3] * 2); } else { truth[3] = "F"; answer.Add(answer[3]); }
                        break;
                    case 3:
                        if (colints.Where((i, j) => j < 3).Where(x => x == 3).Count() == 1) { truth[0] = "T"; answer.Add(answer[0] + (Mathf.Abs(answer[0]) % 10) + ((Mathf.Abs(answer[0]) / 10) % 10) + (Mathf.Abs(answer[0]) / 100)); } else { truth[0] = "F"; answer.Add(answer[0]); }
                        answer.Add(((answer[1] / 100) * 100) + ((answer[1] / 10) % 10) + ((answer[1] % 10) * 10));
                        if (colints[3] % 3 == 0) { truth[2] = "T"; answer.Add(answer[2] * 2); } else { truth[2] = "F"; answer.Add(answer[2]); }
                        if (colints[4] % 3 == 0) { truth[3] = "T"; answer.Add(100 - answer[3]); } else { truth[3] = "F"; answer.Add(answer[3]); }
                        break;
                    case 4:
                        answer.Add((answer[0] * answer[0]) % 100);
                        if (colints.Where(i => i > 2).Distinct().Count() == 3) { truth[1] = "T"; answer.Add(Mathf.Abs((answer[1] / 10) - (answer[1] % 10))); } else { truth[1] = "F"; answer.Add(answer[1] + (Mathf.Abs((answer[0] / 10) - (answer[0] % 10)))); };
                        if (answer[2] / 10 > 5 || answer[2] % 10 > 5) { truth[2] = "T"; answer.Add(answer[2] - 11); } else { truth[2] = "F"; answer.Add(answer[2] + 9); };
                        if (colints.Contains(4)) { truth[3] = "T"; answer.Add(answer[3] + info.GetPortPlateCount() + info.GetIndicators().Count() + info.GetBatteryHolderCount()); } else answer.Add(3);
                        break;
                    default:
                        if (colints.Where(i => i < 5).Contains(colints[5])) { truth[0] = "T"; answer.Add(answer[0] + info.GetSerialNumberNumbers().Sum()); } else { truth[0] = "F"; answer.Add(answer[0] + info.GetSerialNumberNumbers().Aggregate(1, (i, j) => i * j)); }
                        if ((battype == 0 && info.GetBatteryCount() > info.GetBatteryHolderCount()) || (battype == 2 && info.GetBatteryCount() < info.GetBatteryHolderCount() * 2)) { truth[1] = "T"; answer.Add(answer[1] + (answer[2] % 10) * 3); } else { truth[1] = "F"; answer.Add(answer[1]); }
                        answer.Add((answer[2] / 100) + ((answer[2] / 10) % 10) + (answer[2] % 10));
                        if (modsn.Any(i => "IE".Contains(i))) { truth[3] = "T";  answer.Add(answer[3] * 4); } else { truth[3] = "F"; answer.Add(answer[3] * 6); }
                        break;
                }
                Debug.Log("[Knot Wires #" + moduleID + "] " + string.Join("\n[Knot Wires #" + moduleID + "] ", answer.Take(4).Select((x, i) => truth[i] + ": " + x + " \u2192 " + answer[i + 1]).ToArray()));
                answer[0] = Modify(answer[4]);
                Debug.LogFormat("[Knot Wires #{0}] Release the button on the {1}{2} display of the sequence", moduleID, answer[0], new string[10] { "th", "st", "nd", "rd", "th", "th", "th", "th", "th", "th" }[answer[0] % 10]);
                break;
            case 3:
                string[][] mazes = new string[7][] {
                    new string[15] {"XXXXXXXXXXXXXXX",
                                    "X X       X   X",
                                    "X XXXXX XXX X X",
                                    "X       X   X X",
                                    "XXXXX XXX XXX X",
                                    "X X     X   X X",
                                    "X XXX X X X X X",
                                    "X X   X   X   X",
                                    "X X X XXXXX X X",
                                    "X   X     X X X",
                                    "XXX XXX X XXX X",
                                    "X X     X   X X",
                                    "X X XXX XXX X X",
                                    "X     X   X   X",
                                    "XXXXXXXXXXXXXXX"},
                    new string[15] {"XXXXXXXXXXXXXXX",
                                    "X X     X     X",
                                    "X X XXXXX XXX X",
                                    "X   X     X X X",
                                    "X X XXX XXX X X",
                                    "X X   X       X",
                                    "X XXX X XXX XXX",
                                    "X X X   X     X",
                                    "X X XXX XXXXXXX",
                                    "X   X       X X",
                                    "X X X XXX X X X",
                                    "X X   X   X   X",
                                    "XXX XXX XXXXX X",
                                    "X     X     X X",
                                    "XXXXXXXXXXXXXXX"},
                    new string[15] {"XXXXXXXXXXXXXXX",
                                    "X     X     X X",
                                    "XXXXX XXXXX X X",
                                    "X     X     X X",
                                    "X XXX X XXX X X",
                                    "X   X   X     X",
                                    "XXX XXX XXX X X",
                                    "X       X   X X",
                                    "X XXXXX X XXX X",
                                    "X     X   X   X",
                                    "XXXXX X XXX X X",
                                    "X   X       X X",
                                    "XXX X XXXXX XXX",
                                    "X       X     X",
                                    "XXXXXXXXXXXXXXX"},
                    new string[15] {"XXXXXXXXXXXXXXX",
                                    "X     X       X",
                                    "XXXXX X X XXXXX",
                                    "X X     X     X",
                                    "X XXX XXXXX XXX",
                                    "X   X     X   X",
                                    "X XXX X X XXX X",
                                    "X     X X X   X",
                                    "X XXXXXXX X X X",
                                    "X   X X   X X X",
                                    "X X X XXX X XXX",
                                    "X X         X X",
                                    "XXX XXXXX X X X",
                                    "X   X     X   X",
                                    "XXXXXXXXXXXXXXX"},
                    new string[15] {"XXXXXXXXXXXXXXX",
                                    "X   X   X     X",
                                    "XXX X XXX XXX X",
                                    "X   X   X X   X",
                                    "XXX X X X X XXX",
                                    "X X   X   X   X",
                                    "X X X X XXX X X",
                                    "X   X       X X",
                                    "XXX XXX XXX XXX",
                                    "X   X     X   X",
                                    "X XXX XXX X X X",
                                    "X       X   X X",
                                    "XXXXX XXXXX X X",
                                    "X     X     X X",
                                    "XXXXXXXXXXXXXXX"},
                    new string[15] {"XXXXXXXXXXXXXXX",
                                    "X X     X     X",
                                    "X XXX X XXX XXX",
                                    "X     X     X X",
                                    "XXXXX X XXXXX X",
                                    "X     X       X",
                                    "XXX XXXXX X X X",
                                    "X         X X X",
                                    "X XXX XXXXX XXX",
                                    "X X   X       X",
                                    "X X XXX XXXXX X",
                                    "X X     X     X",
                                    "XXX XXX X XXX X",
                                    "X     X   X   X",
                                    "XXXXXXXXXXXXXXX"},
                    new string[15] {"XXXXXXXXXXXXXXX",
                                    "X     X   X   X",
                                    "X X XXX X X X X",
                                    "X X     X   X X",
                                    "XXXXX X XXX X X",
                                    "X     X     X X",
                                    "X XXXXX XXX X X",
                                    "X     X X     X",
                                    "XXX XXX XXX X X",
                                    "X   X     X X X",
                                    "XXX X X XXX XXX",
                                    "X X   X X     X",
                                    "X XXX X X XXX X",
                                    "X     X   X   X",
                                    "XXXXXXXXXXXXXXX"}};
                switch ((rotations[0] - rotations[4] + 4) % 4)
                {
                    case 0:
                        for (int i = 0; i < 15; i++)
                            for (int j = 0; j < 15; j++)
                                maze[i, j] = mazes[colints[5]][i][j];
                        break;
                    case 1:
                        for (int i = 0; i < 15; i++)
                            for (int j = 0; j < 15; j++)
                                maze[i, j] = mazes[colints[5]][j][14 - i];
                        break;
                    case 2:
                        for (int i = 0; i < 15; i++)
                            for (int j = 0; j < 15; j++)
                                maze[i, j] = mazes[colints[5]][14 - i][14 - j];
                        break;
                    default:
                        for (int i = 0; i < 15; i++)
                            for (int j = 0; j < 15; j++)
                                maze[i, j] = mazes[colints[5]][14 - j][i];
                        break;
                }
                if(colints[3] == colints[4])
                {
                    pos[1] = new int[7] { 1, 1, 1, 5, 5, 5, 3 }[colints[3]];
                    pos[0] = new int[7] { 1, 3, 5, 1, 3, 5, 3 }[colints[3]];
                    pos[2] = 3; pos[3] = 3;
                    Debug.LogFormat("[Knot Wires #{0}] The start colour is {1}.", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[3]]);
                    Debug.LogFormat("[Knot Wires #{0}] The exit colour is white.", moduleID);
                }
                else
                {
                    pos[1] = new int[7] { 1, 1, 1, 5, 5, 5, 3 }[colints[3]];
                    pos[0] = new int[7] { 1, 3, 5, 1, 3, 5, 3 }[colints[3]];
                    pos[3] = new int[7] { 1, 1, 1, 5, 5, 5, 3 }[colints[4]];
                    pos[2] = new int[7] { 1, 3, 5, 1, 3, 5, 3 }[colints[4]];
                    Debug.LogFormat("[Knot Wires #{0}] The start colour is {1}.", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[3]]);
                    Debug.LogFormat("[Knot Wires #{0}] The exit colour is {1}.", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white" }[colints[4]]);
                }
                bool[][] locconds = new bool[2][] { new bool[4], new bool[4] };
                if (modsn.Contains("L")) { pos[1]--; locconds[0][0] = true; } else if (modsn.Contains("F")) { pos[1]++; locconds[0][1] = true; }
                if (battype == 1) { pos[0]--; locconds[0][2] = true; } else if (battype == 3) { pos[0]++; locconds[0][3] = true; }
                if (buttonind == 2) { pos[3]--; locconds[1][0] = true; } else if (buttonind == 3) { pos[3]++; locconds[1][1] = true; }
                if (colints.Where((x, i) => i < 3 && x < 3).Count() > 1) { pos[2]--; locconds[1][2] = true; } else if (colints.Where((x, i) => i < 3 && x == 6).Count() == 0) { pos[2]++; locconds[1][3] = true; }
                for (int j = 0; j < 2; j++) {
                    if (!locconds[j].Contains(true)) Debug.LogFormat("[Knot Wires #{0}] No {1} conditions are true.", moduleID, new string[] { "start", "exit"}[j]);
                    else Debug.LogFormat("[Knot Wires #{0}] {1} conditions {2} apply.", moduleID, new string[] { "Start", "Exit" }[j], string.Join(" and ", new string[] { "W", "E", "N", "S" }.Where((x, i) => locconds[j][i]).ToArray())); }
                string[][] logmaze = new string[15][] { new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15], new string[15]};
                for(int i = 0; i < 15; i++)
                    for(int j = 0; j < 15; j++) {
                        if (i % 2 == 0)
                            if (j % 2 == 0) logmaze[i][j] = "╬";
                            else logmaze[i][j] = maze[i, j] == 'X' ? "═" : "┼";
                        else
                            if (j % 2 == 0) logmaze[i][j] = maze[i, j] == 'X' ? "║" : "┼";
                            else logmaze[i][j] = "┼"; }
                if (pos[0] == pos[2] && pos[1] == pos[3])
                    logmaze[(pos[0] * 2) + 1][(pos[1] * 2) + 1] = "U";
                else
                {
                    logmaze[(pos[0] * 2) + 1][(pos[1] * 2) + 1] = "S";
                    logmaze[(pos[2] * 2) + 1][(pos[3] * 2) + 1] = "E";
                }
                Debug.LogFormat("[Knot Wires #{0}] The maze has the following configuration:\n[Knot Wires #{0}] {1}", moduleID, string.Join("\n[Knot Wires #" + moduleID + "] ", logmaze.Select(i => string.Join("", i)).ToArray()));
                if (pos[0] != pos[2] || pos[1] != pos[3])
                {
                    pos[4] = pos[0];
                    pos[5] = pos[1];
                    int[] conds = new int[3];
                    if (colints.Where(x => x == 6).Count() > 1) dir[0] = (rotations[0] - rotations[4] + 6) % 4;
                    else if (colints.Where((x, i) => i < 3 && x == 2).Count() == 0) { conds[0] = 1; dir[0] = (rotations[2] - rotations[4] + 4) % 4; }
                    else if ((rotations[0] - rotations[1] + 4) % 4 == 2) { conds[0] = 2; dir[0] = (rotations[3] - rotations[4] + 4) % 4; }
                    else { conds[0] = 3; dir[0] = 0; }
                    if (battype < 2 && colints[4] < 3) dir[1] = (dir[0] + 2) % 4;
                    else if (battype > 1 && colints[4] > 2 && colints[4] != 6) { conds[1] = 1;  dir[1] = (rotations[0] - rotations[4] + 4) % 4; }
                    else if (colints.Distinct().Count() < 4) { conds[1] = 2; dir[1] = (rotations[3] - rotations[4] + 4) % 4; }
                    else { conds[1] = 3; dir[1] = 1; }
                    if (colints.GroupBy(x => x).Any(k => k.Count() > 2)) dir[2] = (dir[1] + 1) % 4;
                    else if (colints.Where((x, i) => i != 1).Contains(colints[1])) { conds[2] = 1; dir[2] = (dir[0] + 1) % 4; }
                    else if (dir[0] == (rotations[2] - rotations[4] + 4) % 4) { conds[2] = 2; dir[2] = (rotations[1] - rotations[4] + 3) % 4; }
                    else { conds[2] = 3; dir[2] = 3; }
                    for (int i = 0; i < 3; i++)
                        if (conds[i] == 3) Debug.LogFormat("[Knot Wires #{0}] No {1} conditions apply.", moduleID, new string[] { "R/C", "G/M", "B/Y" }[i]);
                        else Debug.LogFormat("[Knot Wires #{0}] {1} condition {2} applies.", moduleID, new string[] { "R/C", "G/M", "B/Y" }[i], conds[i] + 1);
                    for (int i = 1; i < 3; i++)
                        while (dir.Where((x, j) => j < i).Contains(dir[i]))
                            dir[i] = (dir[i] + 1) % 4;
                    while (dir.Where((x, i) => i < 3).Contains(dir[6]))
                        dir[6]++;
                    for (int i = 0; i < 3; i++)
                        dir[i + 3] = dir[i];
                    for (int i = 0; i < 3; i++)
                        Debug.LogFormat("[Knot Wires #{0}] Releasing the button when the display is {1} moves {2}", moduleID, new string[] { "red/cyan", "green/magenta", "blue/yellow" }[i], new string[] { "north", "east", "south", "west" }[dir[i]]);
                    Debug.LogFormat("[Knot Wires #{0}] Tapping the button moves {1}", moduleID, new string[] { "north", "east", "south", "west" }[dir[6]]);
                }
                break;
            case 4:               
                int[] vals = new int[6] { Random.Range(0, 36), Random.Range(0, 36), Random.Range(0, 36), 0, 0, 0 };
                int[] snvals = info.GetSerialNumber().Select(x => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x)).ToArray();
                submissions.Add(colints[snvals[0] % 6]);
                Debug.LogFormat("[Knot Wires #{0}] Initiate sequence by releasing the button on a {1} display. ({2})", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "white"}[submissions[0]], new string[] { "First LED", "Middle LED", "Third LED", "Button", "Battery", "Wires"}[snvals[0] % 6]);
                int ind = new int[] { snvals[0], snvals[1], snvals[3], snvals[4] }[battype];
                for (int i = 0; i < 3; i++)
                {
                    morseflash[i] = morse[vals[i]];
                    switch (colints[i])
                    {
                        case 0: vals[i + 3] = (vals[i] + ind) % 36; break;
                        case 1: vals[i + 3] = (vals[i] + vals[(i + 1) % 3]) % 36; break;
                        case 2: vals[i + 3] = (2 * vals[i] + 36 - ind) % 36; break;
                        case 3: vals[i + 3] = (vals[i] + 36 - ind) % 36; break;
                        case 4: vals[i + 3] = (vals[i] + vals[(i + 2) % 3]) % 36; break;
                        case 5: vals[i + 3] = (2 * ind - vals[i] + 36) % 36; break;
                        default: vals[i + 3] = (ind - vals[i] + 36) % 36; break;
                    }
                }
                Debug.LogFormat("[Knot Wires #{0}] The input characters are: {1}", moduleID, string.Join(" ", vals.Where((x, i) => i < 3).Select(x => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[x].ToString()).ToArray()));
                Debug.LogFormat("[Knot Wires #{0}] The output characters are: {1}", moduleID, string.Join(" ", vals.Where((x, i) => i >= 3).Select(x => "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[x].ToString()).ToArray()));
                string morsestring = string.Join("", vals.Where((x, i) => i >= 3).Select(x => morse[x]).ToArray());
                for (int i = 0; i < 3; i++)
                {
                    morseflash[i] = string.Join("", morseflash[i].Select(x => x == '.' ? "#-" : "###-").ToArray()) + "--";
                    ms[i] = Telseq(morseflash[i], i);
                }
                Debug.LogFormat("[Knot Wires #{0}] The initial string of morse code is \"{1}\".", moduleID, morsestring);
                if(morsestring.Length >= snvals[2] + snvals[5])
                {
                    if (buttonind >= 2)
                        morsestring.Reverse();
                    morsestring = string.Join("", morsestring.Take(snvals[2] + snvals[5]).Reverse().Concat(morsestring.Skip(snvals[2] + snvals[5])).Select(x => x.ToString()).ToArray());
                    if (buttonind >= 2)
                        morsestring.Reverse();
                }
                if (snvals[2] > 0 && snvals[5] > 0)
                {
                    int[] exsn = new int[2] { Math.Min(snvals[2], snvals[5]), Math.Max(snvals[2], snvals[5]) };
                    if (exsn[0] == exsn[1])
                        exsn[0] = 0;
                    morsestring = string.Join("", morsestring.Select((x, i) => ((i + 1) % exsn[1] == exsn[0]) ? (morsestring[i] == '.' ? '-' : '.').ToString() : morsestring[i].ToString()).ToArray());
                }
                if (morsestring.Length % 3 > 0)
                    morsestring += morsestring[0];
                if (morsestring.Length % 3 > 0)
                    morsestring += morsestring[1];
                Debug.LogFormat("[Knot Wires #{0}] The transformed string of morse code is \"{1}\".", moduleID, morsestring);
                for(int i = 0; i < morsestring.Length / 3; i++)
                {
                    string triplet = morsestring.Substring(3 * i, 3);
                    morsesub.Add(Array.IndexOf(new string[8] { "...", "-..", ".-.", "..-", ".--", "-.-", "--.", "---"}, triplet));
                }
                Debug.LogFormat("[Knot Wires #{0}] Submit the sequence: {1}", moduleID, string.Join(", ", morsesub.Select(x => new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[x]).ToArray()));
                break;
            case 5:                
                string[] tapwords = new string[25] { "ACID", "AZON", "BLUE", "COPY", "ECHO", "FIVE", "FOUL", "GREY", "JOIN", "LAND", "LEFT", "MOVE", "NEWS", "ONYX", "PINK", "PORT", "PUSH", "QUAD", "SLOW", "STAR", "THIS", "TICK", "WHAT", "WHEN", "WIRE" };
                int[] tselect = new int[2] { Random.Range(0, 25), Random.Range(0, 24)};
                Debug.LogFormat("[Knot Wires #{0}] The word displayed is {1} and the letter displayed is {2}.", moduleID, tapwords[tselect[0]], "ABCDEFGHIJKLMNOPQRSTUVWX"[tselect[1]]);
                for(int i = 0; i < 4; i++)
                {
                    int let = "ABCDEFGHIJLMNOPQRSTUVWXYZ".IndexOf(tapwords[tselect[0]][i]);
                    if (let == -1) let = 2;
                    tapdiff[2 * i] = (let / 5) + 1;
                    tapdiff[(2 * i) + 1] = (let % 5) + 1;
                }
                digitone = morse[tselect[1] + 10].Select(x => x == '.' ? false : true).ToArray();
                for (int i = 1; i < 4; i++)
                    answer.Add((tselect[0] + (i * (tselect[1] + 1))) % 25);
                Debug.LogFormat("[Knot Wires #{0}] Tap the button when the {1}, {2}, and {3} rules, in order, are satisfied.", moduleID, tapwords[answer[0]], tapwords[answer[1]], tapwords[answer[2]]);
                break;
            case 6:
                List<int> relrots = rotations.Skip(1).Take(3).Select(x => (x - rotations[0] + 4) % 4).ToList();
                string[] vwords = new string[44] { "APPLE", "ANGER", "BRAIN", "BURST", "CAJUN", "CLIFF", "DEMON", "DOUGH", "ENJOY", "EXACT", "FIRST", "FRESH", "GLAZE", "GUILT", "HAVOC", "HOTEL", "INDEX", "IONIC", "JERKY", "JUICE", "KANJI", "KIWIS", "LOOSE", "LUNAR", "MERIT", "MUDDY", "NEWLY", "NOTCH", "ORBIT", "OXIDE", "PLANT", "PRISM", "RADIX", "ROBOT", "SPARK", "SUGAR", "TAWNY", "TRICK", "ULTRA", "UNZIP", "VALUE", "VITRO", "WAXES", "WRITE" };
                int[][] alphcols = new int[26][] { new int[2]{ 0, 1 }, new int[2]{ 0, 2 }, new int[2]{ 0, 3 }, new int[2]{ 0, 4 }, new int[2]{ 0, 5 }, new int[2]{ 0, 6 }, new int[2]{ 1, 2 }, new int[2]{ 1, 3 }, new int[2]{ 1, 4 }, new int[2]{ 1, 5 }, new int[2]{ 1, 6 }, new int[2]{ 2, 3 }, new int[2]{ 2, 4 }, new int[2]{ 2, 5 }, new int[2]{ 2, 6 }, new int[2]{ 3, 4 }, new int[2]{ 3, 5 }, new int[2]{ 3, 6 }, new int[2]{ 4, 5 }, new int[2]{ 4, 6 }, new int[2]{ 5, 6 }, new int[2]{ 0, 0 }, new int[2]{ 1, 1 }, new int[2]{ 3, 3 }, new int[2]{ 5, 5 }, new int[2]{ 6, 6 } };
                string alph = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                int[] wordselect = new int[2];
                wordselect[0] = (info.GetSerialNumberLetters().First() - 'A') + info.GetSerialNumberNumbers().First() + info.GetSerialNumberNumbers().Last();
            triplezero:;
                wordselect[1] = Random.Range(0, 44);
                while (wordselect[1] == wordselect[0])
                    wordselect[1] = Random.Range(0, 44);
                string[] ciphertext = new string[4] { vwords[wordselect[1]], "", "", ""};
                string[] enclogs = new string[4] { "[Knot Wires #" + moduleID + "] The encrypted word is: ", "", "", ""};
                for (int i = 0; i < 3; i++)
                {
                    enclogs[3 - i] = string.Format("The {0} cipher to decrypt is ", new string[] { "third", "second", "first"}[i]);
                    switch (relrots.IndexOf(i + 1))
                    {
                        case 0:
                            enclogs[3 - i] += "Playfair.\n";
                            string playkey = string.Join("", colints.Where((x, j) => j < 3).Select(x => new string[] { "RED", "GREEN", "BLUE", "CYAN", "MAGENTA", "YELLOW", "WHITE" }[x]).ToArray());
                            playkey = new string((playkey + string.Join("", info.GetSerialNumberLetters().Select(x => x.ToString()).ToArray()) + alph).Distinct().ToArray()).Replace("X", "");
                            enclogs[3 - i] += "[Knot Wires #" + moduleID + "] The keysquare used is:\n[Knot Wires #" + moduleID + "] " + string.Join("\n[Knot Wires #" + moduleID + "] ", Enumerable.Range(0, 5).Select(x => string.Join(" ", Enumerable.Range(5 * x, 5).Select(y => playkey[y].ToString()).ToArray())).ToArray());
                            int w = info.GetSerialNumberNumbers().Sum() % 6;
                            enclogs[3 - i] += "\n[Knot Wires #" + moduleID + "] X is inserted at position " + w.ToString() + ".\n";
                            ciphertext[i] = new string(ciphertext[i].Take(w).Concat("X").Concat(ciphertext[i].Skip(w)).ToArray());
                            for(int j = 0; j < 6; j += 2)
                            {
                                int[][] pos = new int[2][] { new int[2], new int[2]};
                                string[] ch = new string[2] { ciphertext[i][j].ToString(), ciphertext[i][j + 1].ToString()};
                                if(ch[0] == "X")
                                {
                                    ciphertext[i + 1] += "X";
                                    if(ch[1] == "X")
                                        ciphertext[i + 1] += "X";
                                    else
                                    {
                                        pos[0][0] = playkey.IndexOf(ch[1].ToString()) / 5;
                                        pos[0][1] = playkey.IndexOf(ch[1].ToString()) % 5;
                                        ciphertext[i + 1] += playkey[(5 * (4 - pos[0][0])) + (4 - pos[0][1])].ToString();
                                    }
                                }
                                else if(ch[1] == "X")
                                {
                                    pos[0][0] = playkey.IndexOf(ch[0].ToString()) / 5;
                                    pos[0][1] = playkey.IndexOf(ch[0].ToString()) % 5;
                                    ciphertext[i + 1] += playkey[(5 * (4 - pos[0][0])) + (4 - pos[0][1])].ToString() + "X";
                                }
                                else if(ch[0] == ch[1])
                                {
                                    pos[0][0] = playkey.IndexOf(ch[0].ToString()) / 5;
                                    pos[0][1] = playkey.IndexOf(ch[0].ToString()) % 5;
                                    string zh = playkey[(5 * (4 - pos[0][0])) + (4 - pos[0][1])].ToString();
                                    ciphertext[i + 1] += zh + zh;
                                }
                                else
                                {
                                    pos[0][0] = playkey.IndexOf(ch[0].ToString()) / 5;
                                    pos[0][1] = playkey.IndexOf(ch[0].ToString()) % 5;
                                    pos[1][0] = playkey.IndexOf(ch[1].ToString()) / 5;
                                    pos[1][1] = playkey.IndexOf(ch[1].ToString()) % 5;
                                    if(pos[0][0] == pos[1][0])
                                    {
                                        ciphertext[i + 1] += playkey[(5 * pos[0][0]) + ((pos[0][1] + 4) % 5)].ToString();
                                        ciphertext[i + 1] += playkey[(5 * pos[0][0]) + ((pos[1][1] + 4) % 5)].ToString();
                                    }
                                    else if(pos[0][1] == pos[1][1])
                                    {
                                        ciphertext[i + 1] += playkey[(5 * ((pos[0][0] + 4) % 5)) + pos[0][1]].ToString();
                                        ciphertext[i + 1] += playkey[(5 * ((pos[1][0] + 4) % 5)) + pos[0][1]].ToString();
                                    }
                                    else
                                    {
                                        ciphertext[i + 1] += playkey[(5 * pos[0][0]) + pos[1][1]].ToString();
                                        ciphertext[i + 1] += playkey[(5 * pos[1][0]) + pos[0][1]].ToString();
                                    }
                                }
                            }
                            ciphertext[i] = ciphertext[i].Remove(w, 1);
                            ciphertext[i + 1] = ciphertext[i + 1].Remove(w, 1);
                            break;
                        case 1:
                            enclogs[3 - i] += "Chain Rotation.\n";
                            ciphertext[i + 1] = alph[((ciphertext[i][0] - 'A') + 51 - (colints[3] + (buttonind * 7))) % 26].ToString();
                            enclogs[3 - i] += "[Knot Wires #" + moduleID + "] " +  ciphertext[i + 1] + " + " + (colints[3] + (buttonind * 7) + 1).ToString() + " = " + ciphertext[i][0] + "\n";
                            for (int j = 1; j < 5; j++)
                            {
                                ciphertext[i + 1] += alph[((ciphertext[i][j] - 'A') + 25 - (ciphertext[i][j - 1] - 'A')) % 26].ToString();
                                enclogs[3 - i] += "[Knot Wires #" + moduleID + "] " + ciphertext[i + 1][j] + " + " + ciphertext[i][j - 1] + " = " + ciphertext[i][j] + "\n";
                            }
                            break;
                        default:
                            enclogs[3 - i] += "Trifid Subtraction.\n";
                            string keyword = vwords[wordselect[0]];
                            enclogs[3 - i] += "[Knot Wires #" + moduleID + "] The keyword is: " + keyword + "\n"; 
                            int[][] trits = new int[3][] { new int[15], new int[15], new int[15] };
                            for(int j = 0; j < 5; j++)
                            {
                                int letterind = ciphertext[i][j] - 'A' + 1;
                                trits[0][j] = letterind / 9;
                                trits[0][j + 5] = (letterind / 3) % 3;
                                trits[0][j + 10] = letterind % 3;
                                letterind = keyword[j] - 'A' + 1;
                                trits[1][j] = letterind / 9;
                                trits[1][j + 5] = (letterind / 3) % 3;
                                trits[1][j + 10] = letterind % 3;
                            }
                            trits[2] = trits[0].Select((x, j) => new int[] { 0, 1, 2, 2, 0, 1, 1, 2, 0}[(3 * trits[1][j]) + x]).ToArray();
                            string[] tritlogs = new string[3] { "[Knot Wires #" + moduleID + "] The deciphered trits are:\n", "[Knot Wires #" + moduleID + "] The keyword trits are:\n", "[Knot Wires #" + moduleID + "] The encrypted trits are:\n", };
                            for (int j = 0; j < 3; j++)
                                for (int k = 0; k < 3; k++)
                                    tritlogs[j] += "[Knot Wires #" + moduleID + "] " + trits[j].Where((x, q) => k == q / 5).Select(x => x.ToString()).Join() + "\n";
                            for (int j = 0; j < 15; j += 3)
                            {
                                if (trits[2][j] == 0 && trits[2][j + 1] == 0 && trits[2][j + 2] == 0)
                                    goto triplezero;
                                ciphertext[i + 1] += alph[(9 * trits[2][j]) + (3 * trits[2][j + 1]) + trits[2][j + 2] - 1].ToString();
                            }
                            for (int j = 0; j < 3; j++)
                                enclogs[3 - i] += tritlogs[2 - j];
                            break;
                    }
                    ciphertext[i + 1] = ciphertext[i + 1].ToUpperInvariant();
                    enclogs[3 - i] = string.Format("[Knot Wires #{0}] {1}[Knot Wires #{0}] {2}.", moduleID, enclogs[3 - i], "Decryption yields: " + ciphertext[i]);
                }
                enclogs[0] += ciphertext[3];
                for (int i = 0; i < 4; i++)
                    Debug.Log(enclogs[i]);
                for (int i = 0; i < 5; i++)
                    colencoding[i] = alphcols[ciphertext[3][i] - 'A'];
                int txind = (info.GetSerialNumberLetters().Take(2).Select(x => x - 'A').Sum() + 2) % 5;
                answer = alphcols[ciphertext[0][txind] - 'A'].ToList();
                Debug.LogFormat("[Knot Wires #{0}] Character {1} of the deciphered word is: {2}", moduleID, txind + 1, ciphertext[0][txind]);
                Debug.LogFormat("[Knot Wires #{0}] Submit the colours {1} and {2}.", moduleID, new string[] { "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White"}[answer[0]], new string[] { "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[answer[1]]);
                break;
            case 7:
                break;
            case 8:
                sounds = Enumerable.Range(0, 30).ToArray().Shuffle().Take(10).ToArray();
                Debug.LogFormat("[Knot Wires #{0}] The digits 0-9 are assigned to the following sounds:\n[Knot Wires #{0}] {1}", moduleID, string.Join("\n[Knot Wires #" + moduleID + "] ", sounds.Select((x, i) => i.ToString() + " = " + new string[30] { "A Mistake", "Battleship", "Boxing", "Broken Buttons", "Cheap Checkout", "Colored Squares", "Creation", "Double Expert", "Double-Oh", "Fast Math", "Graffiti Numbers", "The Heart", "Hogwarts", "The Hypercube", "Laundry", "Painting", "Quiz Buzz", "Qwirkle", "RGB Maze", "RPS Judging", "Simon Shifts", "Simon Shouts", "Sink", "Street Fighter", "The Screw", "The Swan", "Tennis", "Word Search", "Yahtzee", "Zoni"}[x]).ToArray()));
                int[] rc = new int[2];
                numalts[0] = Enumerable.Range(0, 5).Any(x => sounds.Count(y => y % 5 == x) % 5 == 0);
                numalts[1] = Enumerable.Range(0, 6).Any(x => sounds.Count(y => y / 6 == x) % 6 == 0);
                numalts[2] = sounds.Where(x => x < 25 && x % 5 < 4).Any(x => sounds.Contains(x + 1) && sounds.Contains(x + 5) && sounds.Contains(x + 6));
                numalts[3] = sounds.Where(x => x < 20 && x % 5 < 3).Any(x => sounds.Contains(x + 2) && sounds.Contains(x + 10) && sounds.Contains(x + 12));
                numalts[4] = modsn.Skip(2).Any(x => info.GetSerialNumberLetters().Contains(x));
                numalts[5] = sounds.Count(x => x % 2 == 0) > 5;
                numalts[6] = sounds.Count(x => x > 14) > 5;
                numalts[7] = info.GetSerialNumberNumbers().Any(x => x == 0);
                Debug.LogFormat("[Knot Wires #{0}] The alteration rules that apply are: {1}", moduleID, numalts.All(x => x == false) ? "None" : string.Join(", ", Enumerable.Range(1, 8).Where(x => numalts[x - 1]).Select(x => x.ToString()).ToArray()));
                if(!colints.Where((x, i) => i < 3).Any(x => x == 0))
                {
                    if (colints[5] > 4)
                        rc[0] = 1;
                    else if (colints.Where((x, i) => i < 3).Contains(colints[5]))
                        rc[0] = 2;
                    else if (colints[3] == 1 || colints[3] == 4)
                        rc[0] = 3;
                    else if (colints.Where((x, i) => i < 3).Contains(colints[3]))
                        rc[0] = 4;
                    else
                        rc[0] = 5;
                }
                if(!info.GetSerialNumberLetters().Any(x => displays[1].text.Contains(x)))
                {
                    if (colints.Where((x, i) => i < 3).Distinct().Count() < 3)
                        rc[1] = 1;
                    else if (info.GetBatteryCount() < 1)
                        rc[1] = 2;
                    else if (battype == 0)
                        rc[1] = 3;
                    else
                        rc[1] = 4;
                }
                Debug.LogFormat("[Knot Wires #{0}] The {1} column and {2} row are used.", moduleID, new string[] { "first", "second", "third", "fourth", "fifth", "sixth" }[rc[0]], new string[] { "first", "second", "third", "fourth", "fifth" }[rc[1]]);
                pdivs[0] = sounds.Contains(rc[0] * 5 + rc[1]) ? 2 : 3;
                pdivs[1] = new int[] { 5, 7, 11}[sounds.Count(x => x / 5 == rc[0]) / 2];
                pdivs[2] = new int[] { 13, 17, 19, 23 }[sounds.Count(x => x % 5 == rc[1]) / 2];
                Debug.LogFormat("[Knot Wires #{0}] The three prime numbers are: {1}", moduleID, string.Join(", ", pdivs.Select(x => x.ToString()).ToArray()));
                break;
            default:
                inittime = (int)info.GetTime();
                submissions.Add(0);
                submissions.Add(149);
                int[] outs = new int[3] { Random.Range(1, 31),  Random.Range(1, 31), Random.Range(70, 100) };
                initnums[0] = Enumerable.Range(1, 99).Except(outs).ToList().Shuffle().Take(4).ToList();
                initnums[0].Add(outs[0]);
                initnums[0].Add(outs[1]);
                initnums[0].Add(outs[2]);
                initnums[0].Shuffle();
                Debug.LogFormat("[Knot Wires #{0}] The values of each colour are: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 7).Select(x => "RGBCMYW"[x] + "=" + initnums[0][x].ToString()).ToArray()));
                initnums[1] = initnums[0].OrderBy(x => x).Select(x => initnums[0].IndexOf(x)).ToList();
                Debug.LogFormat("[Knot Wires #{0}] The colours, in ascending order of their values, are: {1}", moduleID, string.Join(", ", initnums[1].Select(x => new string[] { "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[x]).ToArray()));
                for (int i = 0; i < 7; i++)
                {
                    switch (initnums[1][i])
                    {
                        case 0:
                            answer.Add(colints.Take(3).Distinct().Count() - 1);
                            answer.Add(Math.Min(2, colints.Count(x => x == initnums[1][6])));
                            break;
                        case 1:
                            if (modsn.Count(x => "AEIOU".Contains(x.ToString())) > 1) answer.Add(3);
                            else if (modsn.All(x => !"DKN".Contains(x.ToString()))) answer.Add(4);
                            else answer.Add(5);
                            answer.Add(3);
                            break;
                        case 2:
                            if (colints[1] < 3) answer.Add(6);
                            else if (colints[0] < 3 || colints[2] < 3) answer.Add(7);
                            else answer.Add(8);
                            if ((rotations[3] - rotations[4] + 4) % 4 == 2) answer.Add(6);
                            else if ((rotations[2] - rotations[4] + 4) % 4 == 2) answer.Add(7);
                            else answer.Add(8);
                            break;
                        case 3:
                            if (buttonind == 3) answer.Add(9);
                            else if (buttonind == 1) answer.Add(10);
                            else answer.Add(11);
                            if (initnums[0][colints[4]] < 31) answer.Add(9);
                            else if (initnums[0][colints[4]] < 61) answer.Add(10);
                            else answer.Add(11);
                            break;
                        case 4:
                            if (battype == 2) answer.Add(12);
                            else if (battype == 0) answer.Add(13);
                            else answer.Add(14);
                            if (info.GetOnIndicators().Join().Any(x => modsn.Skip(2).Any(y => y == x))) answer.Add(12);
                            else if (info.GetOffIndicators().Join().Any(x => modsn.Skip(2).Any(y => y == x))) answer.Add(13);
                            else answer.Add(14);
                            break;
                        case 5:
                            if (colints.All(x => x != 5)) answer.Add(15);
                            else if (colints.Count(x => x > 2 && x < 6) < 4) answer.Add(16);
                            else answer.Add(17);
                            if (initnums[1][6] < 3) answer.Add(15);
                            else if (initnums[1][5] < 3) answer.Add(16);
                            else answer.Add(17);
                            break;
                        default:
                            if (initnums[0][6] % 5 == 0) answer.Add(18);
                            else if (initnums[0][6] % 3 == 0) answer.Add(19);
                            else answer.Add(20);
                            answer.Add(((i + 1) / 3) + 18);
                            break;
                    }
                    Debug.LogFormat("[Knot Wires #{0}] {3}: The {1} hold rule and {2} release rule apply.", moduleID, new string[] { "first", "second", "third"}[answer[answer.Count() - 2] % 3], new string[] { "first", "second", "third"}[answer.Last() % 3], new string[] { "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White"}[initnums[1][i]]);
                }
                break;
        }
        button.OnInteract += delegate ()
        {
            if (!moduleSolved)
            {
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, button.transform);
                if(moduletype == 0)
                    submissions.Add((int)info.GetTime() % 10);
                if((moduletype != 7 && moduletype != 9) || !relseq)
                    StartCoroutine("Display");
                if(moduletype == 9 && relseq)
                {
                    bool next = false;
                    switch (answer[submissions[0]])
                    {
                        case 0: next = ((int)info.GetTime() % 10) == ((int)info.GetTime() % 60) / 10; break;
                        case 1: next = Math.Abs(((int)info.GetTime() % 10) - (((int)info.GetTime() % 60) / 10)) == 1; break;
                        case 2: next = Math.Abs(((int)info.GetTime() % 10) - (((int)info.GetTime() % 60) / 10)) == 2; break;
                        case 3: next = screenints[1] == 1; break;
                        case 4: next = screenints[1] == 5; break;
                        case 5: next = screenints[1] == 0; break;
                        case 6: next = screenints[0] % 7 == 0; break;
                        case 7: next = screenints[0] % 5 == 0; break;
                        case 8: next = screenints[0] % 3 == 0; break;
                        case 9: next = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149}.Contains(screenints[0]); break;
                        case 10: next = new int[] { 4, 6, 9, 10, 14, 15, 21, 22, 25, 26, 33, 34, 35, 38, 39, 46, 49, 51, 55, 57, 58, 62, 65, 69, 74, 77, 82, 85, 86, 87, 91, 93, 94, 95, 106, 111, 115, 118, 119, 121, 122, 123, 129, 133, 134, 141, 142, 143, 145, 146}.Contains(screenints[0]); break;
                        case 11: next = new int[] { 8, 12, 18, 20, 27, 28, 30, 42, 44, 45, 50, 52, 63, 66, 68, 70, 75, 76, 78, 92, 98, 99, 102, 105, 110, 114, 116, 117, 124, 125, 130, 138, 147, 148}.Contains(screenints[0]); break;
                        case 12: next = screenints[0] % 10 == colints.Count(x => x == 0); break;
                        case 13: next = screenints[0] % 10 == colints.Count(x => x == 2); break;
                        case 14: next = screenints[0] % 10 == colints.Count(x => x == 6); break;
                        case 15: next = info.GetFormattedTime().Contains('0'); break;
                        case 16: next = info.GetFormattedTime().Contains('2'); break;
                        case 17: next = info.GetFormattedTime().Contains('4'); break;
                        case 18: next = ((int)info.GetTime() % 10) == info.GetSerialNumberNumbers().Last(); break;
                        case 19: next = ((int)info.GetTime() % 10) == info.GetSerialNumberNumbers().First(); break;
                        default: next = ((int)info.GetTime() % 10) == ((info.GetSerialNumberNumbers().Sum() - 1) % 9) + 1; break;
                    }
                    Debug.LogFormat("[Knot Wires #{0}] Button held: {1} {2} at {3}. {4}.", moduleID, new string[] { "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White"}[screenints[1]], screenints[0], info.GetFormattedTime(), next ? "Correct" : "Incorrect");
                    if (next)
                    {
                        submissions[0]++;
                        submissions[1] = screenints[0];
                    }
                    else
                    {
                        StopCoroutine("Countdown");
                        displays[0].text = "";
                        displays[0].fontSize = cb ? 100 : 200;
                        module.HandleStrike();
                        submissions[0] = 0;
                        submissions[1] = 149;
                        relseq = false;
                        shownum[0] = new bool[7];
                    }
                }
            }
            return false;
        };
        button.OnInteractEnded += delegate ()
        {
            if (!moduleSolved)
            {
                StopCoroutine("Display");
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonRelease, button.transform);
                button.AddInteractionPunch();
                switch (moduletype)
                {
                    case 0:
                        submissions.Add(((screenints[0] - 1) % 9) + 1);
                        if (submissions[0] == answer[0] && submissions[1] == answer[1])
                        {
                            moduleSolved = true;
                            module.HandlePass();
                            Debug.LogFormat("[Knot Wires #{0}] Button was held at {1} and released at {2}. Correct.", moduleID, submissions[0], submissions[1]);
                        }
                        else
                        {
                            module.HandleStrike();
                            Debug.LogFormat("[Knot Wires #{0}] Button was held at {1} and released at {2}. This was incorrect.", moduleID, submissions[0], submissions[1]);
                        }
                        break;
                    case 1:
                        submissions.Add(screenints[0]);
                        if (answer.Contains(submissions[0]))
                        {
                            moduleSolved = true;
                            module.HandlePass();
                            Debug.LogFormat("[Knot Wires #{0}] Submitted {1}: This meets all three conditions.", moduleID, submissions[0]);
                        }
                        else
                        {
                            module.HandleStrike();
                            if (submissions[0] == -1)
                                Debug.LogFormat("[Knot Wires #{0}] Submitted NaN: This fails all conditions.", moduleID);
                            else
                            {
                                int faults = 7 - (4 * Convert.ToInt32(indconds[0][submissions[0] - 1]) + 2 * Convert.ToInt32(indconds[1][submissions[0] - 1]) + Convert.ToInt32(indconds[2][submissions[0] - 1]));
                                Debug.LogFormat("[Knot Wires #{0}] Submitted {1}: This fails to meet condition{2} {3}.", moduleID, submissions[0], new int[] { 1, 2, 4 }.Contains(faults) ? "" : "s", new string[] { "", "3", "2", "2 and 3", "1", "1 and 3", "2 and 3", "1, 2, and 3" }[faults]);
                            }
                        }
                        submissions.Clear();
                        break;
                    case 2:
                        submissions.Add(dispnum);
                        submissions.Add(screenints[0]);
                        if(submissions[0] == answer[0])
                        {
                            moduleSolved = true;
                            module.HandlePass();
                        }
                        else if(submissions[1] != -1)
                            module.HandleStrike();
                        submissions.Clear();
                        break;
                    case 3:
                        if (dir[0] == -1)
                            if (screenints[1] == 6)
                            {
                                moduleSolved = true;
                                module.HandlePass();
                                Debug.LogFormat("[Knot Wires #{0}] Button released on white. Module solved.", moduleID);
                            }
                            else
                            {
                                module.HandleStrike();
                                Debug.LogFormat("[Knot Wires #{0}] Button released on {1}. Special case applies.", moduleID, new string[] { "red", "green", "blue", "cyan", "magenta", "yellow" }[submissions[0]]);
                            }
                        else if (screenints[1] == 6)
                        {
                            pos[4] = pos[0];
                            pos[5] = pos[1];
                            Debug.LogFormat("[Knot Wires #{0}] Button released on white. Resetting.", moduleID);
                        }
                        else
                        {
                            if (screenints[1] == -1)
                                screenints[1] = 6;
                            switch (dir[screenints[1]])
                            {
                                case 0:
                                    if (maze[pos[4] * 2, (pos[5] * 2) + 1] == 'X') { module.HandleStrike(); Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Hit wall north of {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    else { pos[4]--; Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Moving north to {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    break;
                                case 1:
                                    if (maze[(pos[4] * 2) + 1, (pos[5] * 2) + 2] == 'X') { module.HandleStrike(); Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Hit wall east of {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    else { pos[5]++; Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Moving east to {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    break;
                                case 2:
                                    if (maze[(pos[4] * 2) + 2, (pos[5] * 2) + 1] == 'X') { module.HandleStrike(); Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Hit wall south of {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    else { pos[4]++; Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Moving south to {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    break;
                                default:
                                    if (maze[(pos[4] * 2) + 1, pos[5] * 2] == 'X') { module.HandleStrike(); Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Hit wall west of {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    else { pos[5]--; Debug.LogFormat("[Knot Wires #{0}] Button {1}{2}. Moving west to {3}{4}.", moduleID, screenints[1] != 6 ? "released on " : "", new string[] { "red", "green", "blue", "cyan", "magenta", "yellow", "tapped" }[screenints[1]], "ABCDEFG"[pos[5]], pos[4] + 1);}
                                    break;
                            }
                            if(pos[4] == pos[2] && pos[5] == pos[3]) { module.HandlePass(); moduleSolved = true;  Debug.LogFormat("[Knot Wires #{0}] Exit reached.", moduleID);}
                        }
                        break;
                    case 4:
                        if (!relseq)
                        {
                            if(screenints[1] == submissions[0])
                            {
                                relseq = true;
                                for (int i = 0; i < 3; i++)
                                    StartCoroutine(ms[i]);
                                Debug.LogFormat("[Knot Wires #{0}] {1} submitted. Telegraphs acivated.", moduleID, new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[screenints[1] + 1]);
                            }
                            else
                            {
                                Debug.LogFormat("[Knot Wires #{0}] {1} submitted. {2} expected.", moduleID, new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[screenints[1] + 1], new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[morsesub[0]]);
                                module.HandleStrike();
                            }
                        }
                        else
                        {
                            if(screenints[1] == morsesub[0] - 1)
                            {
                                if (morsesub.Count() > 1)
                                {
                                    Debug.LogFormat("[Knot Wires #{0}] {1} submitted. Submit {2} next.", moduleID, new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[morsesub[0]], new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[morsesub[1]]);
                                    morsesub.RemoveAt(0);
                                }
                                else
                                {
                                    Debug.LogFormat("[Knot Wires #{0}] {1} submitted. Sequence complete.", moduleID, new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[morsesub[0]]);
                                    for (int i = 0; i < 3; i++)
                                    {
                                        StopCoroutine(ms[i]);
                                        leds[i].material = lits[colints[i]];
                                    }
                                    moduleSolved = true;
                                    module.HandlePass();
                                }
                            }
                            else
                            {
                                Debug.LogFormat("[Knot Wires #{0}] {1} submitted. {2} expected.", moduleID, new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[screenints[1] + 1], new string[] { "Black", "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[morsesub[0]]);
                                relseq = false;
                                for (int i = 0; i < 3; i++)
                                {
                                    StopCoroutine(ms[i]);
                                    leds[i].material = lits[colints[i]];
                                }
                                module.HandleStrike();
                            }
                        }
                        break;
                    case 5:
                        if(dispnum == 0)
                        {
                            bool accept = false;
                            int time = (int)info.GetTime();
                            int[] timedigs = new int[4] { (time / 600) % 10, (time / 60) % 10, (time % 60) / 10, time % 10};
                            switch (answer[0])
                            {
                                case 0: accept = timedigs[2] + timedigs[3] == 7; break;
                                case 1: accept = timedigs[2] > timedigs[3]; break;
                                case 2: accept = timedigs[3] == colints.Count(x => x == 2); break;
                                case 3: accept = timedigs[2] == timedigs[3]; break;
                                case 4: accept = timedigs[1] == timedigs[3]; break;
                                case 5: accept = timedigs[3] == 5; break;
                                case 6: accept = timedigs[3] == colints.Count(x => x == 0 || x == 5); break;
                                case 7: accept = timedigs[3] == colints.Distinct().Count(); break;
                                case 8: accept = Mathf.Abs(timedigs[2] - timedigs[3]) == 1; break;
                                case 9: accept = timedigs[3] == colints.Count(x => x == 3 || x == 5); break;
                                case 10: accept = timedigs[3] == info.GetSerialNumberNumbers().First(); break;
                                case 11: accept = timedigs.Count(x => x == 0) > 1 || info.GetFormattedTime().Count(x => x == '0') > 1; break;
                                case 12: accept = timedigs[2] + timedigs[3] == new int[] { 9, 11, 3, 5 }[(rotations[0] - rotations[4] + 4) % 4]; break;
                                case 13: accept = info.GetFormattedTime().Select(x => "123456789".Contains(x) ? x - '0' : 0).Sum() < 5 || info.GetFormattedTime().Select(x => "123456789".Contains(x) ? x - '0' : 0).Sum() > 16; break;
                                case 14: accept = timedigs[3] == colints.Count(x => x == 4); break;
                                case 15: accept = timedigs[3] == info.GetPorts().Count() % 10; break;
                                case 16: accept = Mathf.Abs(timedigs[1] - timedigs[3]) == buttonind + 1; break;
                                case 17: accept = timedigs.Where(x => x != 0).GroupBy(x => x).All(x => x.Count() == 1); break;
                                case 18: accept = timedigs[2] == 0; break;
                                case 19: accept = info.GetFormattedTime().Select(x => "123456789".Contains(x) ? x - '0' : 0).Sum() % 5 == 0; break;
                                case 20: accept = info.GetFormattedTime().Select(x => "123456789".Contains(x) ? x - '0' : 0).GroupBy(x => x).All(x => x.Count() != 1 || x.Contains(0)); break;
                                case 21: accept = info.GetFormattedTime().Select(x => "123456789".Contains(x) ? x - '0' : 0).Count(x => x % 2 == 1) % 2 == 1; break;
                                case 22: accept = timedigs[3] == (info.GetSolvedModuleNames().Count() + 1) % 10; break;
                                case 23: accept = Mathf.Abs(timedigs[2] - timedigs[3]) == 4; break;
                                default: accept = timedigs[3] == new int[] { 1, 7, 6, 5, 2, 3, 4 }[colints[5]]; break;
                            }
                            int stage = 3 - answer.Count();
                            Debug.LogFormat("[Knot Wires #{0}] Button tapped at {1}, this {2} the {3} condition.", moduleID, info.GetFormattedTime(), accept ? "satisfies" : "does not satisfy", new string[] { "first", "second", "third" }[stage]);
                            if (accept)
                            {
                                colints[stage] = -1;
                                leds[stage].material = off;
                                if (cb)
                                    cbtexts[stage].text = "";
                                answer.RemoveAt(0);
                                if (stage == 2)
                                {
                                    moduleSolved = true;
                                    module.HandlePass();
                                }
                            }
                            else
                                module.HandleStrike();
                        }
                        break;
                    case 6:
                        if(!relseq)
                        {
                            if (dispnum > 9)
                            {
                                relseq = true;
                                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.Switch, button.transform);
                                Debug.LogFormat("[Knot Wires #{0}] Submission mode activated.", moduleID);
                            }
                        }
                        else
                        {                            
                            if (dispnum > 10)
                            {
                                Debug.LogFormat("[Knot Wires #{0}] Submitted {1}.", moduleID, new string[] {"Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[screenints[1]]);
                                if (answer.Contains(screenints[1]))
                                {
                                    answer.Remove(screenints[1]);
                                    if (answer.Count() == 0)
                                    {
                                        moduleSolved = true;
                                        module.HandlePass();
                                    }
                                }
                                else
                                    module.HandleStrike();
                            }
                            else
                            {
                                module.HandleStrike();
                                Debug.LogFormat("[Knot Wires #{0}] Invalid display submitted.", moduleID);
                            }
                        }
                        break;
                    case 7:
                        if (relseq)
                        {
                            int rel = relnums[dispnum];
                            Debug.LogFormat("[Knot Wires #{0}] Eliminated {1}.", moduleID, rel);
                            if (crelnums.Contains(rel))
                            {
                                displays[0].color = new Color(1, 0, 0);
                                relnums.Remove(rel);
                                if(relnums.All(x => !crelnums.Contains(x)))
                                {
                                    moduleSolved = true;
                                    StopCoroutine("ElimSeq");
                                    displays[0].text = "";
                                    module.HandlePass();
                                }
                            }
                            else
                            {
                                relseq = false;
                                dpairs.Clear();
                                relnums.Clear();
                                StopCoroutine("ElimSeq");
                                displays[0].text = "";
                                module.HandleStrike();
                            }
                            return;
                        }
                        else if(dispnum > 1 && dispnum % 10 == 1)
                        {
                            relseq = true;
                            Debug.LogFormat("[Knot Wires #{0}] The displayed sequence is: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 9).Select(x => new string[] { "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White"}[factors[1, x]] + " " + factors[0, x].ToString()).ToArray()));
                            Debug.LogFormat("[Knot Wires #{0}] The transformed sequence is: {1}", moduleID, string.Join(", ", Enumerable.Range(0, 9).Select(x => factors[2, x].ToString()).ToArray()));
                            dispnum = 0;
                            string p = product.ToString();
                            Debug.LogFormat("[Knot Wires #{0}] The product of these values is: {1}", moduleID, p);
                            for (int i = 0; i < p.Length - 1; i++)
                                dpairs.Add((p[i] - '0') * 10 + (p[i + 1] - '0'));
                            dpairs = dpairs.Distinct().ToList();
                            if (dpairs.Contains(0))
                                dpairs.Remove(0);
                            Debug.LogFormat("[Knot Wires #{0}] The pairs of digits in the product are: {1}", moduleID, string.Join(", ", dpairs.Select(x => x.ToString()).ToArray()));
                            dpairs.Shuffle();
                            StartCoroutine("ElimSeq");
                        }
                        break;
                    case 8:
                        if(dispnum == 0 && !relseq)
                        {
                            relseq = true;
                            digitorder = new int[] { 0, 1, 2 }.Shuffle().ToList();
                            digitorder.Add(0);
                            displays[0].fontSize = 130;
                            displays[0].color = new Color(1, 1, 1);
                            bigdig[0] = Enumerable.Range(0, 5).Select(x => Random.Range(0, 10)).ToArray();
                            Debug.LogFormat("[Knot Wires #{0}] Button tapped. Played sounds {1} corresponding to the number {2}.", moduleID, string.Join(", ", bigdig[0].Select(x => new string[30] { "A Mistake", "Battleship", "Boxing", "Broken Buttons", "Cheap Checkout", "Colored Squares", "Creation", "Double Expert", "Double-Oh", "Fast Math", "Graffiti Numbers", "The Heart", "Hogwarts", "The Hypercube", "Laundry", "Painting", "Quiz Buzz", "Qwirkle", "RGB Maze", "RPS Judging", "Simon Shifts", "Simon Shouts", "Sink", "Street Fighter", "The Screw", "The Swan", "Tennis", "Word Search", "Yahtzee", "Zoni" }[sounds[x]]).ToArray()), string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()));
                            StartCoroutine(Soundseq());
                        }
                        else if (relseq)
                        {
                            digitorder[3]++;
                            if (digitorder[3] == 3)
                            {
                                if (answer.Contains((bigdig[1][0] * 100) + (bigdig[1][1] * 10) + bigdig[1][2]))
                                {
                                    module.HandlePass();
                                    moduleSolved = true;
                                }
                                else
                                {
                                    module.HandleStrike();
                                    relseq = false;
                                    displays[0].fontSize = cb ? 100 : 200;
                                }
                                Debug.LogFormat("[Knot Wires #{0}] Submitted {1}{2}{3}.", moduleID, bigdig[1][0], bigdig[1][1], bigdig[1][2]);
                            }
                            else
                                return;
                        }
                        else
                        {                           
                            if (screenints[0] < 1 || screenints[0] % 11 == 0)
                            {
                                Debug.LogFormat("[Knot Wires #{0}] Button released on an invalid display.", moduleID);
                                module.HandleStrike();
                            }
                            else
                            {
                                Debug.LogFormat("[Knot Wires #{0}] Button released on {1}. Played sounds {2} and {3}.", moduleID, displays[0].text, new string[30] { "A Mistake", "Battleship", "Boxing", "Broken Buttons", "Cheap Checkout", "Colored Squares", "Creation", "Double Expert", "Double-Oh", "Fast Math", "Graffiti Numbers", "The Heart", "Hogwarts", "The Hypercube", "Laundry", "Painting", "Quiz Buzz", "Qwirkle", "RGB Maze", "RPS Judging", "Simon Shifts", "Simon Shouts", "Sink", "Street Fighter", "The Screw", "The Swan", "Tennis", "Word Search", "Yahtzee", "Zoni" }[sounds[screenints[0] / 10]], new string[30] { "A Mistake", "Battleship", "Boxing", "Broken Buttons", "Cheap Checkout", "Colored Squares", "Creation", "Double Expert", "Double-Oh", "Fast Math", "Graffiti Numbers", "The Heart", "Hogwarts", "The Hypercube", "Laundry", "Painting", "Quiz Buzz", "Qwirkle", "RGB Maze", "RPS Judging", "Simon Shifts", "Simon Shouts", "Sink", "Street Fighter", "The Screw", "The Swan", "Tennis", "Word Search", "Yahtzee", "Zoni" }[sounds[screenints[0] % 10]]);
                                Audio.PlaySoundAtTransform("Sound" + sounds[screenints[0] / 10], transform);
                                Audio.PlaySoundAtTransform("Sound" + sounds[screenints[0] % 10], transform);
                            }
                        }
                        break;
                    default:
                        if (relseq)
                        {
                            bool next = false;
                            switch (answer[submissions[0]])
                            {
                                case 0: next = colints.All(x => x != initnums[1][6]); break;
                                case 1: next = colints.All(x => x != initnums[1][3]); break;
                                case 2: next = colints.All(x => x != initnums[1][0]); break;
                                case 3:
                                    if (info.GetTime() / inittime > 2f / 3) { next = submissions[1] - screenints[0] == 6; break; }
                                    else if (info.GetTime() / inittime > 1f / 3) { next = submissions[1] - screenints[0] == 3; break; }
                                    else next = submissions[1] == screenints[0]; break;
                                case 6: next = screenints[1] == colints[3]; break;
                                case 7: next = screenints[1] == colints[4]; break;
                                case 8: next = !colints.Contains(screenints[1]); break;
                                case 9: next = ((int)info.GetTime() % 10) + (((int)info.GetTime() % 60) / 10) == 9; break;
                                case 10: next = ((int)info.GetTime() % 10) + (((int)info.GetTime() % 60) / 10) == 7; break;
                                case 11: next = ((int)info.GetTime() % 10) + (((int)info.GetTime() % 60) / 10) == 5; break;
                                case 12: next = screenints[0] % 10 == initnums[0].OrderBy(x => x).First() % 10; break;
                                case 13: next = screenints[0] % 10 == initnums[0].OrderBy(x => x).Skip(3).First() % 10; break;
                                case 14: next = screenints[0] % 10 == initnums[0].OrderBy(x => x).Last() % 10; break;
                                case 15: next = ((screenints[0] / 10) + (screenints[0] % 10)) % 10 == 5; break;
                                case 16: next = Math.Abs((screenints[0] / 10) - (screenints[0] % 10)) == 5; break;
                                case 17: next = screenints[0] / 10 == info.GetSerialNumberNumbers().Last() || screenints[0] % 10 == info.GetSerialNumberNumbers().Last(); break;
                                case 18: next = initnums[0].Any(x => Math.Abs(x - (screenints[0] % 100)) < 3); break;
                                case 19: next = initnums[0].Any(x => Math.Abs(x - (screenints[0] % 100)) == 3); break;
                                default: next = initnums[0].All(x => Math.Abs(x - (screenints[0] % 100)) > 3); break;
                            }
                            Debug.LogFormat("[Knot Wires #{0}] Button released: {1} {2} at {3}. {4}.", moduleID, new string[] { "Red", "Green", "Blue", "Cyan", "Magenta", "Yellow", "White" }[screenints[1]], screenints[0], info.GetFormattedTime(), next ? "Correct" : "Incorrect");
                            if (next)
                            {
                                submissions[0]++;
                                submissions[1] = screenints[0];
                                if(submissions[0] > 13)
                                {
                                    StopCoroutine("Countdown");
                                    displays[0].text = "";
                                    moduleSolved = true;
                                    module.HandlePass();
                                }
                            }
                            else
                            {
                                StopCoroutine("Countdown");
                                displays[0].text = "";
                                displays[0].fontSize = cb ? 100 : 200;
                                module.HandleStrike();
                                submissions[0] = 0;
                                submissions[1] = 149;
                                relseq = false;
                                shownum[0] = new bool[7];
                            }
                        }
                        else if (shownum[0].All(x => x))
                        {
                            relseq = true;
                            StartCoroutine("Countdown");
                        }
                        break;
                }
                if (moduletype == 0)
                    submissions.Clear();
                screen.material = off;
                displays[0].text = string.Empty;
                dispnum = 0;
                screenints = new int[2] { -1, -1 };
            }
        };
    }

    private IEnumerator Display()
    {
        dispnum = 0;
        List<int> forcenum = new List<int> { };
        List<int> transmission = new List<int> { };
        switch (moduletype)
        {
            case 0:
                forcenum.Add(Random.Range(0, 9));
                break;
            case 2:
                forcenum.Add(Random.Range(0, 5));
                break;
        }
        yield return new WaitForSeconds(0.5f);
        if (moduletype == 5)
        {
            dispnum = Random.Range(0, 8 * digitone.Count());
            screenints[0] = Random.Range(1, 100);
        }
        else if (moduletype == 6)
        {
            for (int i = 0; i < 5; i++)
                if (Random.Range(0, 2) == 0)
                {
                    transmission.Add(colencoding[i][0]);
                    transmission.Add(colencoding[i][1]);
                }
                else
                {
                    transmission.Add(colencoding[i][1]);
                    transmission.Add(colencoding[i][0]);
                }
        }
        else if (moduletype == 9)
            shownum[1] = new bool[7];
        while (true)
        {
            dispnum++;
            if(moduletype == 8 && relseq)
            {
                bigdig[1][digitorder[3]] = dispnum % 10;
                displays[0].text = string.Join("", bigdig[1].Select(x => x.ToString()).ToArray());
            }
            else if ((screenints[0] != -1 && Random.Range(0, 4) == 0 && (moduletype != 2 || dispnum != forcenum[0]) && moduletype != 5 && moduletype != 6 && moduletype != 7) || (moduletype == 7 && dispnum % 10 == 1))
            {
                screenints[0] = -1;
                screenints[1] = Random.Range(0, 7);
                screen.material = lits[screenints[1]];
                displays[0].color = new Color32(0, 0, 0, 255);
                displays[0].text = cb ? "RGBCMYW"[screenints[1]].ToString() : string.Empty;
            }
            else
            {
                switch(moduletype)
                {
                    case 0:
                        if (dispnum % 9 == forcenum[0])
                            screenints[0] = 9 * Random.Range(0, 10) + answer[1];
                        else if (dispnum % 9 == 0)
                            forcenum[0] = Random.Range(0, 9);
                        else
                            screenints[0] = Random.Range(1, 100);
                        screenints[1] = Random.Range(0, 7);
                        break;
                    case 2:
                        if (dispnum == forcenum[0])
                        {
                            screenints[0] = startnum[0];
                            screenints[1] = startnum[1];
                        }
                        else
                        {
                            screenints[0] = Random.Range(1, 100);
                            screenints[1] = Random.Range(0, 7);
                        }
                        break;
                    case 5:
                        int[] digs = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
                        int rep = digitone.Count();
                        int newdig = 0;
                        if(dispnum % (rep + 1) == rep || digitone[dispnum % (rep + 1)])
                        {
                            int ten = screenints[0] / 10;
                            digs.Shuffle();
                            for(int i = 0; i < 10; i++)
                                if(Mathf.Abs(digs[i] - ten) == tapdiff[dispnum % 8])
                                {
                                    newdig = digs[i];
                                    break;
                                }
                            screenints[0] = (newdig * 10) + (screenints[0] % 10);
                        }
                        if (dispnum % (rep + 1) == rep || !digitone[dispnum % (rep + 1)])
                        {
                            int one = screenints[0] % 10;
                            digs.Shuffle();
                            for (int i = 0; i < 10; i++)
                                if (Mathf.Abs(digs[i] - one) == tapdiff[dispnum % 8])
                                {
                                    newdig = digs[i];
                                    break;
                                }
                            screenints[0] = ((screenints[0] / 10) * 10) + newdig;
                        }
                        screenints[1] = Random.Range(0, 7);
                        break;
                    case 6:
                        screenints[0] = Random.Range(1, 100);
                        if (dispnum <= 10)
                            screenints[1] = transmission[dispnum - 1];
                        else
                            screenints[1] = Random.Range(0, 7);
                        break;
                    case 7:
                        screenints[0] = Random.Range(1, 100);
                        screenints[1] = Random.Range(0, 7);
                        if (dispnum % 10 == 2)
                            product = 1;
                        factors[0, (dispnum - 2) % 10] = screenints[0];
                        factors[1, (dispnum - 2) % 10] = screenints[1];
                        if (screenints[1] == colints[0])
                            factors[2, (dispnum - 2) % 10] = ((9 - (screenints[0] / 10)) * 10) + (screenints[0] % 10);
                        else if (screenints[1] == colints[1])
                            factors[2, (dispnum - 2) % 10] = 100 - screenints[0];
                        else if (screenints[1] == colints[2])
                            factors[2, (dispnum - 2) % 10] = (screenints[0] / 10) + 9 - (screenints[0] % 10);
                        else if (screenints[1] == colints[3])
                            factors[2, (dispnum - 2) % 10] = (((screenints[0] / 10) + 5) * 10) + ((screenints[0] + 5) % 10);
                        else if (screenints[1] == colints[4])
                            factors[2, (dispnum - 2) % 10] = (screenints[0] * 3) % 100;
                        else if (screenints[1] == colints[5])
                            factors[2, (dispnum - 2) % 10] = (screenints[0] % 10) * 10 + (screenints[0] / 10);
                        else
                            factors[2, (dispnum - 2) % 10] = screenints[0];
                        product *= factors[2, (dispnum - 2) % 10];
                            break;
                    case 9:
                        screenints[1] = Random.Range(0, 7);
                        if (shownum[1][screenints[1]])
                            screenints[0] = Random.Range(1, 100);
                        else
                        {
                            shownum[0][screenints[1]] = true;
                            shownum[1][screenints[1]] = true;
                            screenints[0] = initnums[0][screenints[1]];
                        }
                        break;
                    default:
                        screenints[0] = Random.Range(1, 100);
                        screenints[1] = Random.Range(0, 7);
                        break;
                }
                screen.material = off;
                displays[0].text = (screenints[0] < 10 ? "0" : "") + screenints[0].ToString() + (cb ? " " + "RGBCMYW"[screenints[1]].ToString() : "");
                displays[0].color = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(75, 75, 255, 255), new Color32(0, 255, 255, 255), new Color32(255, 0, 255, 255), new Color32(255, 255, 0, 255), new Color32(255, 255, 255, 255) }[screenints[1]];
            }
            yield return new WaitForSeconds(moduletype == 7 ? 1.25f : 0.75f);
        }
    }

    private int Modify(int x)
    {
        x = Mathf.Abs(x);
        while (x < 10)
            x += 20;
        while (x > 40)
            x -= 20;
        return x;
    }

    private IEnumerator Telseq(string m, int i)
    {
        for(int j = Random.Range(0, m.Length); j < m.Length; j++)
        {
            leds[i].material = m[j] == '#' ? lits[colints[i]] : off;
            yield return new WaitForSeconds(0.2f);
            if(j == m.Length - 1)
                j = -1;
        }
    }

    private IEnumerator Soundseq()
    {
        moduleSolved = true;
        for(int i = 0; i < 5; i++)
        {
            Audio.PlaySoundAtTransform("Sound" + sounds[bigdig[0][i]], transform);
            yield return new WaitForSeconds(1);
        }
        displays[0].text = "000";
        answer.Clear();
        bigdig[1] = new int[3];
        string tlog = string.Join("", bigdig[0].Select(x => x.ToString()).ToArray());
        int temp = 0;
        if (numalts[0]) { temp = bigdig[0][0]; bigdig[0][0] = bigdig[0][1]; bigdig[0][1] = temp; tlog += " \u2192 " + string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()); }
        if (numalts[1]) { temp = bigdig[0][3]; bigdig[0][3] = bigdig[0][4]; bigdig[0][4] = temp; tlog += " \u2192 " + string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()); }
        if (numalts[2]) { temp = bigdig[0][0]; bigdig[0][0] = bigdig[0][4]; bigdig[0][4] = temp; temp = bigdig[0][1]; bigdig[0][3] = bigdig[0][1]; bigdig[0][3] = temp; tlog += " \u2192 " + string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()); }
        if (numalts[3]) { for (int i = 0; i < 5; i++) bigdig[0][i] = 9 - bigdig[0][i]; tlog += " \u2192 " + string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()); }
        if (numalts[4]) { temp = bigdig[0][1]; bigdig[0][1] = bigdig[0][3]; bigdig[0][3] = temp; tlog += " \u2192 " + string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()); }
        if (numalts[5]) { temp = bigdig[0][0]; bigdig[0][0] = bigdig[0][2]; bigdig[0][2] = temp; tlog += " \u2192 " + string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()); }
        if (numalts[6]) { temp = bigdig[0][2]; bigdig[0][2] = bigdig[0][4]; bigdig[0][4] = temp; tlog += " \u2192 " + string.Join("", bigdig[0].Select(x => x.ToString()).ToArray()); }
        int wangernum = (bigdig[0][0] * 10000) + (bigdig[0][1] * 1000) + (bigdig[0][2] * 100) + (bigdig[0][3] * 10) + bigdig[0][4];
        if (numalts[7]) { wangernum += 11111; tlog += " \u2192 " + wangernum; }
        if (wangernum < 1000) wangernum += 1000;
        Debug.LogFormat("[Knot Wires #{0}] Applying transformations: {1}", moduleID, tlog);
        int[] rems = new int[3];
        for (int i = 0; i < 3; i++)
        {
            rems[i] = wangernum % pdivs[i];
            Debug.LogFormat("[Knot Wires #{0}] {1} mod {2} = {3}", moduleID, wangernum, pdivs[i], rems[i]);
        }
        for (int i = 0; i < 1000; i++)
            if (i % pdivs[0] == rems[0] && i % pdivs[1] == rems[1] && i % pdivs[2] == rems[2])
                answer.Add(i);
        Debug.LogFormat("[Knot Wires #{0}] Valid submissions are: {1}", moduleID, string.Join(", ", answer.Select(x => x.ToString()).ToArray()));
        moduleSolved = false;
    }

    private IEnumerator ElimSeq()
    {
        bool eliminc = dpairs.Count() < 6 || Random.Range(0, 2) == 0;
        List<int> rem = Enumerable.Range(0, 100).ToList().Except(dpairs).ToList().Shuffle();
        relnums = (dpairs.Count() < 6 ? dpairs : dpairs.Take(eliminc ? 5 : 6)).Concat(rem.Take(eliminc ? 6 : 5)).ToList().Shuffle();
        Debug.LogFormat("[Knot Wires #{0}] The sequence displays the numbers: {1}", moduleID, string.Join(", ", relnums.OrderBy(x => x).Select(x => x.ToString()).ToArray()));
        crelnums = (eliminc ? (dpairs.Count() < 6 ? dpairs : dpairs.Take(5)) : rem.Take(5)).ToArray();
        Debug.LogFormat("[Knot Wires #{0}] Eliminate the numbers: {1}", moduleID, string.Join(", ", crelnums.Select(x => x.ToString()).OrderBy(x => x).ToArray()));
        while (true)
        {
            int num = relnums[dispnum];
            displays[0].color = new Color(1, 1, 1);
            displays[0].text = (num < 10 ? "0" : "") + num.ToString();
            yield return new WaitForSeconds(0.66f);
            dispnum++;
            if(dispnum >= relnums.Count() - 1)
            {
                dispnum = 0;
                relnums.Shuffle();
            }
        }
    }

    private IEnumerator Countdown()
    {
        int[] r = new int[10];
        displays[0].fontSize = cb ? 70 : 130;
        yield return null;
        for (int i = 149; i >= 0; i--)
        {
            screenints[0] = i;
            if(i % 10 == 9)
                r = new int[10] { 0, 1, 2, 3, 4, 5, 6, Random.Range(0, 7), Random.Range(0, 7), Random.Range(0, 7) };
            screenints[1] = r[i % 10];
            displays[0].text = (screenints[0] < 100 ? "0" : (screenints[0] < 10 ? "00" : "")) + screenints[0].ToString() + (cb ? " " + "RGBCMYW"[screenints[1]].ToString() : "");
            displays[0].color = new Color32[] { new Color32(255, 0, 0, 255), new Color32(0, 255, 0, 255), new Color32(75, 75, 255, 255), new Color32(0, 255, 255, 255), new Color32(255, 0, 255, 255), new Color32(255, 255, 0, 255), new Color32(255, 255, 255, 255) }[screenints[1]];
            yield return new WaitForSeconds(1);
        }
        Debug.LogFormat("[Knot Wires #{0}] Time's Up.", moduleID);
        displays[0].text = "";
        displays[0].fontSize = cb ? 100 : 200;
        module.HandleStrike();
        relseq = false;
        shownum[0] = new bool[7];
    }
}
