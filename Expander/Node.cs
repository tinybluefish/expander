using System;
using System.Collections.Generic;
using System.Text;

namespace Expander
{
    // If this is just a single term, possibilities are:
    // 1: 'RIC1' -> an external code (i.e. not BLTS), e.g. the RIC1 in GetDSS('RIC1')
    // 2: 1 -> a single integer
    // 3: BCodeX -> a BLTS code name, which needs expansion
    // 4: 'BCodeX' -> a quoted BLTS code, which would get loaded by BDQ directly, but we want to expand? 
    // In all cases, just check and see if it's expandable or not
    // Strip quotes from this term if present, otherwise if it's an expandable code we'll miss it.
    public class Node
    {
        public const string MAX_DEPTH = "maxdepth";
        public const string NODE_COUNT = "nodecount";

        // Refactor to a different class
        public static Dictionary<string, string> lookup = new Dictionary<string, string>();

        // This can either be the name of the Function or the value of the Term this node represents.
        public string name { get; set; }
        public List<Node> children = new List<Node>();
        public int level;
        // Need to be able to reset this between different code parses
        public static Dictionary<string, int> stats = new Dictionary<string, int>() { { MAX_DEPTH, 0 }, { NODE_COUNT, 0 } };

        public Node(int _level, string _name)
        {
            this.name = _name;
            this.level = _level;

            if (!stats.ContainsKey(name))
            {
                stats[name] = 0;
            }
            stats[name]++;

            stats[MAX_DEPTH] = Math.Max(stats[MAX_DEPTH], level);
            stats[NODE_COUNT]++;
        }

        // Full expression, the pointer to the current position in the expression, and the level this node is at.
        public static Node ParseExpression(string rootExpression, ref int pointer, int level)
        {
            Node node;

            // Expressions are broken up with parens and commas, let's see what we've got.
            int firstLeftParen = rootExpression.IndexOf('(', pointer);
            int firstRightParen = rootExpression.IndexOf(')', pointer);
            int firstComma = rootExpression.IndexOf(',', pointer);

            bool leftParenFirst = firstLeftParen >= 0 && (firstRightParen < 0 || firstLeftParen < firstRightParen) && (firstComma < 0 || firstLeftParen < firstComma);
            bool rightParenFirst = firstRightParen >= 0 && (firstLeftParen < 0 || firstRightParen < firstLeftParen) && (firstComma < 0 || firstRightParen < firstComma);
            bool commaFirst = firstComma >= 0 && (firstLeftParen < 0 || firstComma < firstLeftParen) && (firstRightParen < 0 || firstComma < firstRightParen);

            // If left paren first then...
            //   grab the term, increase level, descend...
            if (leftParenFirst)
            {
                node = new Node(level, rootExpression.Substring(pointer, firstLeftParen - pointer).Trim());
                if (firstRightParen == firstLeftParen + 1) // Special case, no-arg func call
                {
                    pointer = rootExpression.IndexOf(')', firstRightParen);
                }
                // Regular case
                else
                {
                    pointer = firstLeftParen; // move along 
                                              // parse all the arguments of this expression, the pointer will be moved to ) when we get to the end

                    do
                    {
                        pointer++; // position from the comma/) it's pointing to the next expr
                        Node child = ParseExpression(rootExpression, ref pointer, level + 1);
                        node.children.Add(child);
                    }
                    while (rootExpression[pointer] == ',');

                    // tricksy, need to move pointer to next block...
                    pointer++;
                }
            }
            // if right paren first => grab the term, move the pointer on, return...
            else if (rightParenFirst)
            {
                node = new Node(level, rootExpression.Substring(pointer, firstRightParen - pointer).Trim('\'', '"', ' '));
                // If we've got an expansion for this term, then grab it from the lookup and kick off a new sub-tree,
                // but starting at the current level.
                if (lookup.ContainsKey(node.name))
                {
                    int subPointer = 0; // Do we care about this..?
                    node.children.Add(ParseExpression(lookup[node.name], ref subPointer, level + 1));
                }
                // Otherwise, this is a bare term, so no kids here, just return.
                pointer = firstRightParen;
            }
            // if comma first => grab the term, maintain level, move pointer on, return
            else if (commaFirst)
            {
                node = new Node(level, rootExpression.Substring(pointer, firstComma - pointer).Trim('\'', '"', ' '));
                // If we've got an expansion for this term, then grab it from the lookup and kick off a new sub-tree,
                // but starting at the current level.
                if (lookup.ContainsKey(node.name))
                {
                    int subPointer = 0; // Do we care about this..?
                    node.children.Add(ParseExpression(lookup[node.name], ref subPointer, level + 1));
                }
                // Otherwise, this is a bare term, so no kids here, just return.
                pointer = firstComma;
            }
            // If there's nothing here then we must be looking at a bare term, which *may* be expandable.
            else
            {
                node = new Node(level, rootExpression.Trim('\'', '"', ' '));
                pointer = rootExpression.Length - 1;
            }

            return node;
        }

        public static string GetStats()
        {
            StringBuilder sb = new StringBuilder("Tree stats:\n");

            foreach (KeyValuePair<string, int> pair in Node.stats)
            {
                sb.AppendFormat("\t{0}=\t{1}\n", pair.Key, pair.Value);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            string indent = new string(' ', level * 2);
            sb.Append(indent).AppendFormat("LVL={0} NAME={1}\n", level, name);
            if (children.Count > 0)
            {
                sb.Append(indent).Append("{\n");
                foreach (Node child in children)
                {
                    sb.Append(child);
                }
                sb.Append(indent).Append("}\n");
            }
            return sb.ToString();
        }
    }

}
