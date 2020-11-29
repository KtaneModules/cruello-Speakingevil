using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CruelloScript : MonoBehaviour
{

    public KMAudio Audio;
    public KMBombModule module;
    public KMSelectable[] buttons;
    public Renderer[] brends;
    public Renderer[] brings;
    public Renderer[] checkrends;
    public Material[] mats;
    public Material[] litmats;
    public TextMesh[] blabels;
    public TextMesh[] checklabels;
    public GameObject matstore;

    private int[,,] nums = new int[6, 6, 3];
    private int[] sums = new int[12];
    private int[] colconds = new int[12];
    private bool[,] locked = new bool[6, 6];
    private int[,] bstates = new int[6, 6];
    private string[] sol = new string[6];
    private IEnumerator press;
    private bool held;

    private static int moduleIDCounter;
    private int moduleID;
    private bool moduleSolved;

    private void Awake()
    {
        moduleID = ++moduleIDCounter;
        foreach (KMSelectable b in buttons)
        {
            int i = Array.IndexOf(buttons, b);
            b.OnInteract += delegate ()
            {
                if (!moduleSolved)
                {
                    if (i == 36) Check();
                    else
                    {
                        press = Press(i);
                        StartCoroutine(press);
                        held = false;
                        b.AddInteractionPunch(0.5f);
                    }
                }
                return false;
            };
            b.OnInteractEnded += delegate ()
            {
                if (!moduleSolved && i != 36)
                {
                    StopCoroutine(press);
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, b.transform);
                    if (!held && !locked[i / 6, i % 6])
                    {
                        bstates[i / 6, i % 6] = (bstates[i / 6, i % 6] + 1) % 3;
                        brends[i].material = mats[bstates[i / 6, i % 6]];
                        blabels[i].color = new Color32[] { new Color32(102, 0, 134, 255), new Color32(178, 151, 0, 255), new Color32(0, 134, 123, 255) }[bstates[i / 6, i % 6]];
                        blabels[i].text = nums[i / 6, i % 6, bstates[i / 6, i % 6]].ToString();
                    }
                }
            };
        }
        int y = new int[] { 1, 1, 1, 1, 1, 1, 2, 2, 2, 3, 3, 4 }[Random.Range(0, 12)];
        int[][] shuffle = new int[2][];
        for (int j = 0; j < y; j++)
        {
            for (int k = 0; k < 2; k++)
                shuffle[k] = new int[] { 0, 1, 2, 3, 4, 5 }.Shuffle();
            for (int i = 0; i < 6; i++)
                bstates[shuffle[0][i], shuffle[1][i]] = 1;
        }
        y = Random.Range(1, 5 - y);
        for (int j = 0; j < y; j++)
        {
            for (int k = 0; k < 2; k++)
                shuffle[k] = new int[] { 0, 1, 2, 3, 4, 5 }.Shuffle();
            while (shuffle[0].Where((x, i) => bstates[x, shuffle[1][i]] == 1).Any())
                for (int k = 0; k < 2; k++)
                    shuffle[k] = new int[] { 0, 1, 2, 3, 4, 5 }.Shuffle();
            for (int i = 0; i < 6; i++)
                bstates[shuffle[0][i], shuffle[1][i]] = 2;
        }
        for (int i = 0; i < 36; i++)
        {
            int[] r = new int[3] { Random.Range(0, 10), Random.Range(0, 10), Random.Range(0, 10) };
            for (int j = 0; j < 3; j++)
            {
                while (r.Where((x, k) => k < j).Contains(r[j]))
                    r[j] = Random.Range(0, 10);
                nums[i / 6, i % 6, j] = r[j];
            }
            blabels[i].text = r[0].ToString();
        }
        string[][][] loggrid = new string[3][][] { new string[6][] { new string[6], new string[6], new string[6], new string[6], new string[6], new string[6] }, new string[6][] { new string[6], new string[6], new string[6], new string[6], new string[6], new string[6] }, new string[6][] { new string[6], new string[6], new string[6], new string[6], new string[6], new string[6] } };
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sums[i] += nums[j, i, bstates[j, i]];
                sums[i + 6] += nums[i, j, bstates[i, j]];
                for (int k = 0; k < 3; k++)
                    loggrid[k][i][j] = nums[i, j, k].ToString();
                sol[i] += ' ';
                sol[i] += "MYC"[bstates[i, j]];
            }
            checklabels[i].text = (sums[i] < 10 ? "0" : "") + sums[i].ToString();
            checklabels[i + 6].text = (sums[i + 6] < 10 ? "0" : "") + sums[i + 6].ToString();
            int[][] gridcols = new int[2][] { new int[3], new int[3] };
            for (int j = 0; j < 6; j++)
            {
                gridcols[0][bstates[j, i]]++;
                gridcols[1][bstates[i, j]]++;
            }
            for (int j = 0; j < 2; j++)
                if (!gridcols[j].All(x => x == 2))
                {
                    colconds[(6 * j) + i] = Array.IndexOf(gridcols[j], Mathf.Max(gridcols[j])) + 1;
                    checklabels[(6 * j) + i].color = new Color32[] { new Color32(255, 0, 255, 255), new Color32(255, 255, 0, 255), new Color32(0, 255, 255, 255) }[colconds[(6 * j) + i] - 1];
                }
        }
        for (int k = 0; k < 3; k++)
            Debug.LogFormat("[Cruello #{0}] The grid of {2} digits is:\n[Cruello #{0}] {1}", moduleID, string.Join("\n[Cruello #" + moduleID + "] ", loggrid[k].Select(x => string.Join(" ", x)).ToArray()), new string[] { "magenta", "yellow", "cyan" }[k]);
        Debug.LogFormat("[Cruello #{0}] The required column sums are: {1}", moduleID, string.Join(", ", sums.Where((x, i) => i < 6).Select(i => i.ToString()).ToArray()));
        Debug.LogFormat("[Cruello #{0}] The required row sums are: {1}", moduleID, string.Join(", ", sums.Where((x, i) => i > 5).Select(i => i.ToString()).ToArray()));
        Debug.LogFormat("[Cruello #{0}] The column colours are: {1}", moduleID, string.Join(", ", colconds.Where((x, i) => i < 6).Select(i => "WMYC"[i].ToString()).ToArray()));
        Debug.LogFormat("[Cruello #{0}] The row colours are: {1}", moduleID, string.Join(", ", colconds.Where((x, i) => i > 5).Select(i => "WMYC"[i].ToString()).ToArray()));
        Debug.LogFormat("[Cruello #{0}] One possible solution is:\n[Cruello #{0}] {1}", moduleID, string.Join("\n[Cruello #" + moduleID + "] ", sol));
        matstore.SetActive(false);
        for (int i = 0; i < 36; i++)
            bstates[i / 6, i % 6] = 0;
    }

    private IEnumerator Press(int i)
    {
        if (i != 36)
        {
            yield return new WaitForSeconds(0.5f);
            held = true;
            locked[i / 6, i % 6] ^= true;
            Audio.PlaySoundAtTransform("Flag", transform);
            brings[i].material = litmats[locked[i / 6, i % 6] ? 1 : 0];
        }
        yield return null;
    }

    private void Check()
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[36].transform);
        int[] calcsum = new int[12];
        bool[] checksums = new bool[12];
        bool[] checkcols = new bool[12];
        for (int j = 0; j < 6; j++)
        {
            for (int k = 0; k < 6; k++)
            {
                calcsum[j] += nums[k, j, bstates[k, j]];
                calcsum[j + 6] += nums[j, k, bstates[j, k]];
            }
            int[][] gridcols = new int[2][] { new int[3], new int[3] };
            for (int k = 0; k < 6; k++)
            {
                gridcols[0][bstates[k, j]]++;
                gridcols[1][bstates[j, k]]++;
            }
            for (int k = 0; k < 2; k++)
                if (!gridcols[k].All(x => x == 2))
                    checkcols[(6 * k) + j] = Array.IndexOf(gridcols[k], Mathf.Max(gridcols[k])) + 1 == colconds[(6 * k) + j];
                else
                    checkcols[(6 * k) + j] = colconds[(6 * k) + j] == 0;
            checksums[j] = calcsum[j] == sums[j];
            checksums[j + 6] = calcsum[j + 6] == sums[j + 6];
            checkrends[j].material = litmats[checksums[j] && checkcols[j] ? 2 : 3];
            checkrends[j + 6].material = litmats[checksums[j + 6] && checkcols[j + 6] ? 2 : 3];
        }
        Debug.LogFormat("[Cruello #{0}] The submitted column sums are: {1}", moduleID, string.Join(", ", calcsum.Where((x, i) => i < 6).Select(i => i.ToString()).ToArray()));
        Debug.LogFormat("[Cruello #{0}] The submitted row sums are: {1}", moduleID, string.Join(", ", calcsum.Where((x, i) => i > 5).Select(i => i.ToString()).ToArray()));
        Debug.LogFormat("[Cruello #{0}] Columns {1} satisfy their colour conditions.", moduleID, string.Join(", ", new string[] { "a", "b", "c", "d", "e", "f" }.Where((x, i) => checkcols[i]).ToArray()));
        Debug.LogFormat("[Cruello #{0}] Rows {1} satisfy their colour conditions.", moduleID, string.Join(", ", new string[] { "1", "2", "3", "4", "5", "6" }.Where((x, i) => checkcols[i + 6]).ToArray()));
        if (checksums.Contains(false) || checkcols.Contains(false))
            module.HandleStrike();
        else
        {
            moduleSolved = true;
            module.HandlePass();
            foreach (Renderer r in brings)
                r.material = litmats[0];
            foreach (TextMesh r in blabels.Union(checklabels))
                r.text = "\u2713";
        }
    }

#pragma warning disable 414
    private string TwitchHelpMessage = "!{0} toggle <a-f><1-6> [Press buttons] | !{0} flag <a-f><1-6> [Holds buttons] | !{0} toggle all [Press every unflagged button] | [Chain toggles or flags by separating the coordinates with spaces] | !{0} submit";
#pragma warning restore 414
    private IEnumerator ProcessTwitchCommand(string command)
    {
        command = command.ToLowerInvariant();
        if (command == "submit")
        {
            yield return null;
            buttons[36].OnInteract();
        }
        else if (command == "toggle all")
        {
            for (int i = 0; i < 36; i++)
                if (!locked[i / 6, i % 6])
                {
                    yield return null;
                    buttons[i].OnInteract();
                    yield return null;
                    buttons[i].OnInteractEnded();
                }
        }
        else
        {
            string[] commands = command.Replace(",", "").Split();
            int[,] prompts = new int[command.Length - 1, 2];
            if (commands[0] != "toggle" && commands[0] != "flag")
            {
                yield return "sendtochaterror!f Invalid command: " + commands[0];
                yield break;
            }
            bool flag = commands[0] == "flag";
            for (int i = 1; i < commands.Length; i++)
            {
                if (commands[i].Length != 2 || !"abcdef".Contains(commands[i][0].ToString()) || !"123456".Contains(commands[i][1]))
                {
                    yield return "sendtochaterror!f Invalid command: " + commands[i];
                    yield break;
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                        prompts[i - 1, j] = "abcdef123456".IndexOf(commands[i][j].ToString()) - (6 * j);
                }
            }
            for (int i = 0; i < commands.Length - 1; i++)
            {
                int b = (6 * prompts[i, 1]) + prompts[i, 0];
                yield return null;
                buttons[b].OnInteract();
                yield return new WaitForSeconds(flag ? 0.6f : 0.1f);
                buttons[b].OnInteractEnded();
            }
        }
    }
    private IEnumerator TwitchHandleForcedSolve()
    {
        int[,] parsedSol = new int[6, 6];
        for (int i = 0; i < 6; i++)
        {
            string[] strSol = sol[i].Trim().Split(' ');
            for (int j = 0; j < 6; j++)
                parsedSol[i, j] = ("MYC".IndexOf(strSol[j]) - bstates[i, j] + 3) % 3;
        }
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (locked[i, j])
                {
                    buttons[i * 6 + j].OnInteract();
                    while (!held) { yield return true; }
                    buttons[i * 6 + j].OnInteractEnded();
                }
                for (int k = 0; k < parsedSol[i, j]; k++)
                {
                    yield return null;
                    buttons[i * 6 + j].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                    buttons[i * 6 + j].OnInteractEnded();
                }
            }
        }
        yield return null;
        buttons[36].OnInteract();
    }
}
