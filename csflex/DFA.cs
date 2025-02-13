// #define DEBUG_TRACE
/* * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
 * C# Flex 1.4                                                             *
 * Copyright (C) 2004-2005  Jonathan Gilbert <logic@deltaq.org>            *
 * Derived from:                                                           *
 *                                                                         *
 *   JFlex 1.4                                                             *
 *   Copyright (C) 1998-2004  Gerwin Klein <lsf@jflex.de>                  *
 *   All rights reserved.                                                  *
 *                                                                         *
 * This program is free software; you can redistribute it and/or modify    *
 * it under the terms of the GNU General Public License. See the file      *
 * COPYRIGHT for more information.                                         *
 *                                                                         *
 * This program is distributed in the hope that it will be useful,         *
 * but WITHOUT ANY WARRANTY; without even the implied warranty of          *
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the           *
 * GNU General Public License for more details.                            *
 *                                                                         *
 * You should have received a copy of the GNU General Public License along *
 * with this program; if not, write to the Free Software Foundation, Inc., *
 * 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA                 *
 *                                                                         *
 * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * */
namespace CSFlex;

using System.Text;
/**
 * DFA representation in C# Flex.
 * Contains minimization algorithm.
 *
 * @author Gerwin Klein
 * @version JFlex 1.4, $Revision: 2.6 $, $Date: 2004/04/12 10:07:48 $
 * @author Jonathan Gilbert
 * @version CSFlex 1.4
 */
public sealed class DFA
{
    /**
     * The initial number of states 
     */
    private const int STATES = 500;
    /**
     * The code for "no target state" in the transition table.
     */
    public const int NO_TARGET = -1;
    /**
     * table[current_state][character] is the next state for <code>current_state</code>
     * with input <code>character</code>, <code>NO_TARGET</code> if there is no transition for
     * this input in <code>current_state</code>
     */
    private int[][] table;
    /**
     * <code>isFinal[state] == true</code> <=> the state <code>state</code> is 
     * a final state.
     */
    private bool[] isFinal;
    /**
     * <code>isPushback[state] == true</code> <=> the state <code>state</code> is 
     * a final state of an expression that can only be matched when followed by
     * a certain lookaead.
     */
    private bool[] isPushback;
    /**
     * <code>isLookEnd[state] == true</code> <=> the state <code>state</code> is 
     * a final state of a lookahead expression.
     */
    private bool[] isLookEnd;
    /**
     * <code>action[state]</code> is the action that is to be carried out in
     * state <code>state</code>, <code>null</code> if there is no action.
     */
    private Action[] action;
    /**
     * lexState[i] is the start-state of lexical state i
     */
    private int[] lexState;
    /**
     * The number of states in this DFA
     */
    private int numStates;
    /**
     * The current maximum number of input characters
     */
    private int numInput;
    /**
     * all actions that are used in this DFA
     */
    private readonly PrettyDictionary<Action, Action> usedActions = new();
    public int[][] Table => table;
    public bool[] IsFinal => isFinal;
    public bool[] IsPushback => isPushback;
    public bool[] IsLookEnd => isLookEnd;
    public Action[] Action => action;
    public int[] LexState => lexState;
    public int NumStates => numStates;
    public int NumInput => numInput;

#if DEBUG_TRACE
System.IO.StreamWriter log = new StreamWriter(@"x:\dfa_csharp.log");
#endif // DEBUG_TRACE

    public DFA(int numLexStates, int numInp)
    {
#if DEBUG_TRACE
log.AutoFlush = true;
log.WriteLine("new DFA(int numLexStates = {0}, int numInp = {1})", numLexStates, numInp);
#endif // DEBUG_TRACE

        numInput = numInp;

        int statesNeeded = Math.Max(numLexStates, STATES);

        table = new int[statesNeeded][];
        for (int i = 0; i < table.Length; i++)
            table[i] = new int[numInput];
        action = new Action[statesNeeded];
        isFinal = new bool[statesNeeded];
        isPushback = new bool[statesNeeded];
        isLookEnd = new bool[statesNeeded];
        lexState = new int[numLexStates];
        numStates = 0;

        for (int i = 0; i < statesNeeded; i++)
        {
            for (char j = (char)0; j < numInput; j++)
                table[i][j] = NO_TARGET;
        }
    }


    public void SetLexState(int lState, int trueState)
    {
        lexState[lState] = trueState;
    }

    private void EnsureStateCapacity(int newNumStates)
    {
        int oldLength = isFinal.Length;
        if (newNumStates < oldLength) return;
        int newLength = oldLength * 2;
        while (newLength <= newNumStates) newLength *= 2;

        var newFinal = new bool[newLength];
        var newPushback = new bool[newLength];
        var newLookEnd = new bool[newLength];
        var newAction = new Action[newLength];
        var newTable = new int[newLength][];
        for (int idx = 0; idx < newTable.Length; idx++)
            newTable[idx] = new int[numInput];

        Array.Copy(isFinal, 0, newFinal, 0, numStates);
        Array.Copy(isPushback, 0, newPushback, 0, numStates);
        Array.Copy(isLookEnd, 0, newLookEnd, 0, numStates);
        Array.Copy(action, 0, newAction, 0, numStates);
        Array.Copy(table, 0, newTable, 0, oldLength);

        int i, j;

        for (i = oldLength; i < newLength; i++)
        {
            for (j = 0; j < numInput; j++)
            {
                newTable[i][j] = NO_TARGET;
            }
        }

        isFinal = newFinal;
        isPushback = newPushback;
        isLookEnd = newLookEnd;
        action = newAction;
        table = newTable;
    }

    public void SetAction(int state, Action? stateAction)
    {
#if DEBUG_TRACE
log.WriteLine("setAction(int state = {0}, Action stateAction = {1})", state, stateAction);
#endif // DEBUG_TRACE

        action[state] = stateAction!;
        if (stateAction != null)
        {
            isLookEnd[state] = stateAction.LookAction;
            usedActions[stateAction] = stateAction;
#if DEBUG_TRACE
  log.WriteLine("  new usedActions size: {0} entries", usedActions.Count);
#endif // DEBUG_TRACE
        }
    }

    public void SetFinal(int state, bool isFinalState)
    {
        isFinal[state] = isFinalState;
    }

    public void SetPushback(int state, bool isPushbackState)
    {
        isPushback[state] = isPushbackState;
    }
    public void AddTransition(int start, char input, int dest)
    {
        int max = Math.Max(start, dest) + 1;
        EnsureStateCapacity(max);
        if (max > numStates) numStates = max;

        //  Out.debug("Adding DFA transition ("+start+", "+(int)input+", "+dest+")");

        table[start][input] = dest;
    }
    public override string ToString()
    {
        var result = new StringBuilder();

        for (int i = 0; i < numStates; i++)
        {
            result.Append("State ");
            if (isFinal[i]) result.Append("[FINAL] "); // (action "+action[i].priority+")] ");
            if (isPushback[i]) result.Append("[PUSH] ");
            result.Append(i).Append(":").Append(OutputWriter.NewLine);

            for (char j = (char)0; j < numInput; j++)
            {
                if (table[i][j] >= 0)
                    result.Append("  with ").Append((int)j)
                          .Append(" in ").Append(table[i][j]).Append(OutputWriter.NewLine);
            }
        }

        return result.ToString();
    }
    public void WriteDot(File? file)
    {
        if (file == null) return;
        try
        {
            var writer = new StreamWriter(file);
            writer.WriteLine(DotFormat());
            writer.Close();
        }
        catch (IOException)
        {
            OutputWriter.Error(ErrorMessages.FILE_WRITE, file);
            throw new GeneratorException();
        }
    }
    public string DotFormat()
    {
        var result = new StringBuilder();

        result.Append("digraph DFA {").Append(OutputWriter.NewLine);
        result.Append("rankdir = LR").Append(OutputWriter.NewLine);

        for (int i = 0; i < numStates; i++)
        {
            if (isFinal[i] || isPushback[i]) result.Append(i);
            if (isFinal[i]) result.Append(" [shape = doublecircle]");
            if (isPushback[i]) result.Append(" [shape = box]");
            if (isFinal[i] || isPushback[i]) result.Append(OutputWriter.NewLine);
        }

        for (int i = 0; i < numStates; i++)
        {
            for (int input = 0; input < numInput; input++)
            {
                if (table[i][input] >= 0)
                {
                    result.Append(i).Append(" -> ").Append(table[i][input]);
                    result.Append(" [label=\"[").Append(input).Append("]\"]").Append(OutputWriter.NewLine);
                    //          result.append(" [label=\"["+classes.toString(input)+"]\"]\n");
                }
            }
        }

        result.Append('}').Append(OutputWriter.NewLine);
        return result.ToString();
    }

    // check if all actions can actually be matched in this DFA
    public void CheckActions(LexScan scanner, LexParse? parser)
    {
        var eofActions = parser?.EOFActions;

        foreach(var next in scanner.actions)
        {
            if (!next.Equals(usedActions[next]) && !eofActions!.IsEOFAction(next))
                OutputWriter.Warning(scanner.file!, ErrorMessages.NEVER_MATCH, (next).Priority - 1, -1);
        }
    }

    /**
     * Implementation of Hopcroft's O(n log n) minimization algorithm, follows
     * description by D. Gries.
     *
     * Time: O(n log n)
     * Space: O(c n), size < 4*(5*c*n + 13*n + 3*c) byte
     */
    public void Minimize()
    {
        int i, j;

        OutputWriter.Print(numStates + " states before minimization, ");

        if (numStates == 0)
        {
            OutputWriter.Error(ErrorMessages.ZERO_STATES);
            throw new GeneratorException();
        }

        if (Options.NoMinimize)
        {
            OutputWriter.Println("minimization skipped.");
            return;
        }

        // the algorithm needs the DFA to be total, so we add an error state 0,
        // and translate the rest of the states by +1
        int n = numStates + 1;

        // block information:
        // [0..n-1] stores which block a state belongs to,
        // [n..2*n-1] stores how many elements each block has
        int[] block = new int[2 * n];

        // implements a doubly linked list of states (these are the actual blocks)
        int[] b_forward = new int[2 * n];
        int[] b_backward = new int[2 * n];

        // the last of the blocks currently in use (in [n..2*n-1])
        // (end of list marker, points to the last used block)
        int lastBlock = n;  // at first we start with one empty block
        int b0 = n;   // the first block    

        // the circular doubly linked list L of pairs (B_i, c)
        // (B_i, c) in L iff l_forward[(B_i-n)*numInput+c] > 0 // numeric value of block 0 = n!
        int[] l_forward = new int[n * numInput + 1];
        int[] l_backward = new int[n * numInput + 1];
        int anchorL = n * numInput; // list anchor

        // inverse of the transition table
        // if t = inv_delta[s][c] then { inv_delta_set[t], inv_delta_set[t+1], .. inv_delta_set[k] }
        // is the set of states, with inv_delta_set[k] = -1 and inv_delta_set[j] >= 0 for t <= j < k  
        int[][] inv_delta = new int[n][];
        for (int idx = 0; idx < inv_delta.Length; idx++)
            inv_delta[idx] = new int[numInput];
        int[] inv_delta_set = new int[2 * n * numInput];

        // twin stores two things: 
        // twin[0]..twin[numSplit-1] is the list of blocks that have been split
        // twin[B_i] is the twin of block B_i
        int[] twin = new int[2 * n];
        int numSplit;

        // SD[B_i] is the the number of states s in B_i with delta(s,a) in B_j
        // if SD[B_i] == block[B_i], there is no need to split
        int[] SD = new int[2 * n]; // [only SD[n..2*n-1] is used]

        // for fixed (B_j,a), the D[0]..D[numD-1] are the inv_delta(B_j,a)
        int[] D = new int[n];
        int numD;

        // initialize inverse of transition table
        int lastDelta = 0;
        int[] inv_lists = new int[n]; // holds a set of lists of states
        int[] inv_list_last = new int[n]; // the last element
        for (int c = 0; c < numInput; c++)
        {
            // clear "head" and "last element" pointers
            for (int s = 0; s < n; s++)
            {
                inv_list_last[s] = -1;
                inv_delta[s][c] = -1;
            }

            // the error state has a transition for each character into itself
            inv_delta[0][c] = 0;
            inv_list_last[0] = 0;

            // accumulate states of inverse delta into lists (inv_delta serves as head of list)
            for (int s = 1; s < n; s++)
            {
                int t = table[s - 1][c] + 1;

                if (inv_list_last[t] == -1)
                { // if there are no elements in the list yet
                    inv_delta[t][c] = s;  // mark t as first and last element
                    inv_list_last[t] = s;
                }
                else
                {
                    inv_lists[inv_list_last[t]] = s; // link t into chain
                    inv_list_last[t] = s; // and mark as last element
                }
            }

            // now move them to inv_delta_set in sequential order, 
            // and update inv_delta accordingly
            for (int s = 0; s < n; s++)
            {
                int i_ = inv_delta[s][c]; inv_delta[s][c] = lastDelta;
                int j_ = inv_list_last[s];
                bool go_on = (i_ != -1);
                while (go_on)
                {
                    go_on = (i_ != j_);
                    inv_delta_set[lastDelta++] = i_;
                    i_ = inv_lists[i_];
                }
                inv_delta_set[lastDelta++] = -1;
            }
        } // of initialize inv_delta

        // printInvDelta(inv_delta, inv_delta_set);

        // initialize blocks 

        // make b0 = {0}  where 0 = the additional error state
        b_forward[b0] = 0;
        b_backward[b0] = 0;
        b_forward[0] = b0;
        b_backward[0] = b0;
        block[0] = b0;
        block[b0] = 1;

        for (int s = 1; s < n; s++)
        {
            // System.out.println("Checking state ["+(s-1)+"]");
            // search the blocks if it fits in somewhere
            // (fit in = same pushback behavior, same finalness, same lookahead behavior, same action)
            int b = b0 + 1; // no state can be equivalent to the error state
            bool found = false;
            while (!found && b <= lastBlock)
            {
                // get some state out of the current block
                int t = b_forward[b];
                // System.out.println("  picking state ["+(t-1)+"]");

                // check, if s could be equivalent with t
                found = (isPushback[s - 1] == isPushback[t - 1]) && (isLookEnd[s - 1] == isLookEnd[t - 1]);
                if (found)
                {
                    if (isFinal[s - 1])
                    {
                        found = isFinal[t - 1] && action[s - 1].IsEquiv(action[t - 1]);
                    }
                    else
                    {
                        found = !isFinal[t - 1];
                    }

                    if (found)
                    { // found -> add state s to block b
                      // System.out.println("Found! Adding to block "+(b-b0));
                      // update block information
                        block[s] = b;
                        block[b]++;

                        // chain in the new element
                        int last = b_backward[b];
                        b_forward[last] = s;
                        b_forward[s] = b;
                        b_backward[b] = s;
                        b_backward[s] = last;
                    }
                }

                b++;
            }

            if (!found)
            { // fits in nowhere -> create new block
              // System.out.println("not found, lastBlock = "+lastBlock);

                // update block information
                block[s] = b;
                block[b]++;

                // chain in the new element
                b_forward[b] = s;
                b_forward[s] = b;
                b_backward[b] = s;
                b_backward[s] = b;

                lastBlock++;
            }
        } // of initialize blocks

        // printBlocks(block,b_forward,b_backward,lastBlock);

        // initialize worklist L
        // first, find the largest block B_max, then, all other (B_i,c) go into the list
        int B_max = b0;
        int B_i;
        for (B_i = b0 + 1; B_i <= lastBlock; B_i++)
            if (block[B_max] < block[B_i]) B_max = B_i;

        // L = empty
        l_forward[anchorL] = anchorL;
        l_backward[anchorL] = anchorL;

        // set up the first list element
        if (B_max == b0) B_i = b0 + 1; else B_i = b0; // there must be at least two blocks    

        int index = (B_i - b0) * numInput;  // (B_i, 0)
        while (index < (B_i + 1 - b0) * numInput)
        {
            int last = l_backward[anchorL];
            l_forward[last] = index;
            l_forward[index] = anchorL;
            l_backward[index] = last;
            l_backward[anchorL] = index;
            index++;
        }

        // now do the rest of L
        while (B_i <= lastBlock)
        {
            if (B_i != B_max)
            {
                index = (B_i - b0) * numInput;
                while (index < (B_i + 1 - b0) * numInput)
                {
                    int last = l_backward[anchorL];
                    l_forward[last] = index;
                    l_forward[index] = anchorL;
                    l_backward[index] = last;
                    l_backward[anchorL] = index;
                    index++;
                }
            }
            B_i++;
        }
        // end of setup L

        // start of "real" algorithm
        // int step = 0;
        // System.out.println("max_steps = "+(n*numInput));
        // while L not empty
        while (l_forward[anchorL] != anchorL)
        {
            // System.out.println("step : "+(step++));
            // printL(l_forward, l_backward, anchorL);

            // pick and delete (B_j, a) in L:

            // pick
            int B_j_a = l_forward[anchorL];
            // delete 
            l_forward[anchorL] = l_forward[B_j_a];
            l_backward[l_forward[anchorL]] = anchorL;
            l_forward[B_j_a] = 0;
            // take B_j_a = (B_j-b0)*numInput+c apart into (B_j, a)
            int B_j = b0 + B_j_a / numInput;
            int a = B_j_a % numInput;

            // printL(l_forward, l_backward, anchorL);      

            // System.out.println("picked ("+B_j+","+a+")");
            // printL(l_forward, l_backward, anchorL);

            // determine splittings of all blocks wrt (B_j, a)
            // i.e. D = inv_delta(B_j,a)
            numD = 0;
            int s = b_forward[B_j];
            while (s != B_j)
            {
                // System.out.println("splitting wrt. state "+s);
                int t = inv_delta[s][a];
                // System.out.println("inv_delta chunk "+t);
                while (inv_delta_set[t] != -1)
                {
                    // System.out.println("D+= state "+inv_delta_set[t]);
                    D[numD++] = inv_delta_set[t++];
                }
                s = b_forward[s];
            }

            // clear the twin list
            numSplit = 0;

            // System.out.println("splitting blocks according to D");

            // clear SD and twins (only those B_i that occur in D)
            for (int indexD = 0; indexD < numD; indexD++)
            { // for each s in D
                s = D[indexD];
                B_i = block[s];
                SD[B_i] = -1;
                twin[B_i] = 0;
            }

            // count how many states of each B_i occuring in D go with a into B_j
            // Actually we only check, if *all* t in B_i go with a into B_j.
            // In this case SD[B_i] == block[B_i] will hold.
            for (int indexD = 0; indexD < numD; indexD++)
            { // for each s in D
                s = D[indexD];
                B_i = block[s];

                // only count, if we haven't checked this block already
                if (SD[B_i] < 0)
                {
                    SD[B_i] = 0;
                    int t = b_forward[B_i];
                    while (t != B_i && (t != 0 || block[0] == B_j) &&
                           (t == 0 || block[table[t - 1][a] + 1] == B_j))
                    {
                        SD[B_i]++;
                        t = b_forward[t];
                    }
                }
            }

            // split each block according to D      
            for (int indexD = 0; indexD < numD; indexD++)
            { // for each s in D
                s = D[indexD];
                B_i = block[s];

                // System.out.println("checking if block "+(B_i-b0)+" must be split because of state "+s);        

                if (SD[B_i] != block[B_i])
                {
                    // System.out.println("state "+(s-1)+" must be moved");
                    int B_k = twin[B_i];
                    if (B_k == 0)
                    {
                        // no twin for B_i yet -> generate new block B_k, make it B_i's twin            
                        B_k = ++lastBlock;
                        // System.out.println("creating block "+(B_k-n));
                        // printBlocks(block,b_forward,b_backward,lastBlock-1);
                        b_forward[B_k] = B_k;
                        b_backward[B_k] = B_k;

                        twin[B_i] = B_k;

                        // mark B_i as split
                        twin[numSplit++] = B_i;
                    }
                    // move s from B_i to B_k

                    // remove s from B_i
                    b_forward[b_backward[s]] = b_forward[s];
                    b_backward[b_forward[s]] = b_backward[s];

                    // add s to B_k
                    int last = b_backward[B_k];
                    b_forward[last] = s;
                    b_forward[s] = B_k;
                    b_backward[s] = last;
                    b_backward[B_k] = s;

                    block[s] = B_k;
                    block[B_k]++;
                    block[B_i]--;

                    SD[B_i]--;  // there is now one state less in B_i that goes with a into B_j
                                // printBlocks(block, b_forward, b_backward, lastBlock);
                                // System.out.println("finished move");
                }
            } // of block splitting
            // printBlocks(block, b_forward, b_backward, lastBlock);
            // System.out.println("updating L");
            // update L
            for (int indexTwin = 0; indexTwin < numSplit; indexTwin++)
            {
                B_i = twin[indexTwin];
                int B_k = twin[B_i];
                for (int c = 0; c < numInput; c++)
                {
                    int B_i_c = (B_i - b0) * numInput + c;
                    int B_k_c = (B_k - b0) * numInput + c;
                    if (l_forward[B_i_c] > 0)
                    {
                        // (B_i,c) already in L --> put (B_k,c) in L
                        int last = l_backward[anchorL];
                        l_backward[anchorL] = B_k_c;
                        l_forward[last] = B_k_c;
                        l_backward[B_k_c] = last;
                        l_forward[B_k_c] = anchorL;
                    }
                    else
                    {
                        // put the smaller block in L
                        if (block[B_i] <= block[B_k])
                        {
                            int last = l_backward[anchorL];
                            l_backward[anchorL] = B_i_c;
                            l_forward[last] = B_i_c;
                            l_backward[B_i_c] = last;
                            l_forward[B_i_c] = anchorL;
                        }
                        else
                        {
                            int last = l_backward[anchorL];
                            l_backward[anchorL] = B_k_c;
                            l_forward[last] = B_k_c;
                            l_backward[B_k_c] = last;
                            l_forward[B_k_c] = anchorL;
                        }
                    }
                }
            }
        }

        // System.out.println("Result");
        // printBlocks(block,b_forward,b_backward,lastBlock);

        /*
        System.out.println("Old minimization:");
        boolean [] [] equiv = old_minimize();

        boolean error = false;
        for (int i = 1; i < equiv.length; i++) {
          for (int j = 0; j < equiv[i].length; j++) {
            if (equiv[i][j] != (block[i+1] == block[j+1])) {
              System.out.println("error: equiv["+i+"]["+j+"] = "+equiv[i][j]+
                                 ", block["+i+"] = "+block[i+1]+", block["+j+"] = "+block[j]);
              error = true;
            }
          }
        }

        if (error) System.exit(1);
        System.out.println("check");
        */

        // transform the transition table 

        // trans[i] is the state j that will replace state i, i.e. 
        // states i and j are equivalent
        int[] trans = new int[numStates];

        // kill[i] is true iff state i is redundant and can be removed
        bool[] kill = new bool[numStates];

        // move[i] is the amount line i has to be moved in the transition table
        // (because states j < i have been removed)
        int[] move = new int[numStates];

        // fill arrays trans[] and kill[] (in O(n))
        for (int b = b0 + 1; b <= lastBlock; b++)
        { // b0 contains the error state
          // get the state with smallest value in current block
            int s = b_forward[b];
            int min_s = s; // there are no empty blocks!
            for (; s != b; s = b_forward[s])
                if (min_s > s) min_s = s;
            // now fill trans[] and kill[] for this block 
            // (and translate states back to partial DFA)
            min_s--;
            for (s = b_forward[b] - 1; s != b - 1; s = b_forward[s + 1] - 1)
            {
                trans[s] = min_s;
                kill[s] = s != min_s;
            }
        }

        // fill array move[] (in O(n))
        int amount = 0;
        for (int idx = 0; idx < numStates; idx++)
        {
            if (kill[idx])
                amount++;
            else
                move[idx] = amount;
        }

        // j is the index in the new transition table
        // the transition table is transformed in place (in O(c n))
        for (i = 0, j = 0; i < numStates; i++)
        {
            // we only copy lines that have not been removed
            if (!kill[i])
            {
                // translate the target states 
                for (int c = 0; c < numInput; c++)
                {
                    if (table[i][c] >= 0)
                    {
                        table[j][c] = trans[table[i][c]];
                        table[j][c] -= move[table[j][c]];
                    }
                    else
                    {
                        table[j][c] = table[i][c];
                    }
                }

                isFinal[j] = isFinal[i];
                isPushback[j] = isPushback[i];
                isLookEnd[j] = isLookEnd[i];
                action[j] = action[i];

                j++;
            }
        }

        numStates = j;

        // translate lexical states
        for (i = 0; i < lexState.Length; i++)
        {
            lexState[i] = trans[lexState[i]];
            lexState[i] -= move[lexState[i]];
        }

        OutputWriter.Println(numStates + " states in minimized DFA");
    }

    public string ToString(int[] a)
    {
        var r = "{";
        int i;
        for (i = 0; i < a.Length - 1; i++) r += a[i] + ",";
        return r + a[i] + "}";
    }

    public void PrintBlocks(int[] b, int[] b_f, int[] b_b, int last)
    {
        OutputWriter.Dump("block     : " + ToString(b));
        OutputWriter.Dump("b_forward : " + ToString(b_f));
        OutputWriter.Dump("b_backward: " + ToString(b_b));
        OutputWriter.Dump("lastBlock : " + last);
        int n = numStates + 1;
        for (int i = n; i <= last; i++)
        {
            OutputWriter.Dump("Block " + (i - n) + " (size " + b[i] + "):");
            string line = "{";
            int s = b_f[i];
            while (s != i)
            {
                line += (s - 1);
                int t = s;
                s = b_f[s];
                if (s != i)
                {
                    line = line + ",";
                    if (b[s] != i) OutputWriter.Dump("consistency error for state " + (s - 1) + " (block " + b[s] + ")");
                }
                if (b_b[s] != t) OutputWriter.Dump("consistency error for b_back in state " + (s - 1) + " (back = " + b_b[s] + ", should be = " + t + ")");
            }
            OutputWriter.Dump(line + "}");
        }
    }

    public void PrintL(int[] l_f, int[] l_b, int anchor)
    {
        var l = "L = {";
        int bc = l_f[anchor];
        while (bc != anchor)
        {
            int b = bc / numInput;
            int c = bc % numInput;
            l += "(" + b + "," + c + ")";
            int old_bc = bc;
            bc = l_f[bc];
            if (bc != anchor) l += ",";
            if (l_b[bc] != old_bc) OutputWriter.Dump("consistency error for (" + b + "," + c + ")");
        }
        OutputWriter.Dump(l + "}");
    }


    public bool[][]? Old_minimize()
    {
        int i, j;
        char c;

        OutputWriter.Print(numStates + " states before minimization, ");

        if (numStates == 0)
        {
            OutputWriter.Error(ErrorMessages.ZERO_STATES);
            throw new GeneratorException();
        }

        if (Options.NoMinimize)
        {
            OutputWriter.Println("minimization skipped.");
            return null;
        }

        // notequiv[i][j] == true <=> state i and state j are equivalent
        bool[][] equiv = new bool[numStates][];

        // list[i][j] contains all pairs of states that have to be marked "not equivalent"
        // if states i and j are recognized to be not equivalent
        StatePairList[][] list = new StatePairList[numStates][];

        // construct a triangular matrix equiv[i][j] with j < i
        // and mark pairs (final state, not final state) as not equivalent
        for (i = 1; i < numStates; i++)
        {
            list[i] = new StatePairList[i];
            equiv[i] = new bool[i];
            for (j = 0; j < i; j++)
            {
                // i and j are equivalent, iff :
                // i and j are both final and their actions are equivalent and have same pushback behaviour or
                // i and j are both not final

                if (isFinal[i] && isFinal[j] && (isPushback[i] == isPushback[j]) && (isLookEnd[i] == isLookEnd[j]))
                    equiv[i][j] = action[i].IsEquiv(action[j]) && !Options.EmitCSharp; // C# #line directives get messed up by merged states
                else
                    equiv[i][j] = !isFinal[j] && !isFinal[i] && (isPushback[i] == isPushback[j]) && (isLookEnd[i] == isLookEnd[j]);
            }
        }


        for (i = 1; i < numStates; i++)
        {

            OutputWriter.Debug("Testing state " + i);

            for (j = 0; j < i; j++)
            {

                if (equiv[i][j])
                {

                    for (c = (char)0; c < numInput; c++)
                    {

                        if (equiv[i][j])
                        {

                            int p = table[i][c];
                            int q = table[j][c];
                            if (p < q)
                            {
                                int t = p;
                                p = q;
                                q = t;
                            }
                            if (p >= 0 || q >= 0)
                            {
                                // Out.debug("Testing input '"+c+"' for ("+i+","+j+")");
                                // Out.debug("Target states are ("+p+","+q+")");
                                if (p != q && (p == -1 || q == -1 || !equiv[p][q]))
                                {
                                    equiv[i][j] = false;
                                    if (list[i][j] != null) list[i][j].MarkAll(list, equiv);
                                }
                                // printTable(equiv);
                            } // if (p >= 0) ..
                        } // if (equiv[i][j]
                    } // for (char c = 0; c < numInput ..

                    // if i and j are still marked equivalent..

                    if (equiv[i][j])
                    {

                        // Out.debug("("+i+","+j+") are still marked equivalent");

                        for (c = (char)0; c < numInput; c++)
                        {

                            int p = table[i][c];
                            int q = table[j][c];
                            if (p < q)
                            {
                                int t = p;
                                p = q;
                                q = t;
                            }

                            if (p != q && p >= 0 && q >= 0)
                            {
                                if (list[p][q] == null)
                                {
                                    list[p][q] = new StatePairList();
                                }
                                list[p][q].AddPair(i, j);
                            }
                        }
                    }
                    else
                    {
                        // Out.debug("("+i+","+j+") are not equivalent");
                    }

                } // of first if (equiv[i][j])
            } // of for j
        } // of for i
          // }

        // printTable(equiv); 

        return equiv;
    }

    public void PrintInvDelta(int[][] inv_delta, int[] inv_delta_set)
    {
        OutputWriter.Dump("Inverse of transition table: ");
        for (int s = 0; s < numStates + 1; s++)
        {
            OutputWriter.Dump("State [" + (s - 1) + "]");
            for (int c = 0; c < numInput; c++)
            {
                string line = "With <" + c + "> in {";
                int t = inv_delta[s][c];
                while (inv_delta_set[t] != -1)
                {
                    line += inv_delta_set[t++] - 1;
                    if (inv_delta_set[t] != -1) line += ",";
                }
                if (inv_delta_set[inv_delta[s][c]] != -1)
                    OutputWriter.Dump(line + "}");
            }
        }
    }

    public void PrintTable(bool[][] equiv)
    {

        OutputWriter.Dump("Equivalence table is : ");
        for (int i = 1; i < numStates; i++)
        {
            string line = i + " :";
            for (int j = 0; j < i; j++)
            {
                if (equiv[i][j])
                    line += " E";
                else
                    line += " x";
            }
            OutputWriter.Dump(line);
        }
    }
}
