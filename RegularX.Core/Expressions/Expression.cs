using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegularX.Core.Expressions
{

    public class Expression
    {
        public string name;
        public string tag;
        public string content;
        public Func<string, string, string> func;
        public Func<string, bool> check;

        public Expression(string name, string tag, Func<string, string, string> func, Func<string, bool> check)
        {
            this.name = name;
            this.tag = tag;
            this.func = func;
            this.check = check;
        }
    }

    public enum ExpressionUseType
    {
        Name, Tag
    }

    public static class ExpressionType
    {
        /// <summary>
        /// Returns a new string based on the pattern used.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string Exp(this string input, string pattern)
        {
            string newString = "";
            Expression[] expressions = FindInString(pattern).ToArray();
            foreach (Expression ex in expressions)
            {
                newString += ex.func.Invoke(input, ex.content);
            }
            return newString;
        }

        /// <summary>
        /// SingleOp will only check the next character of pattern
        /// </summary>
        public static Expression[] simpleOps = new Expression[] 
        {
            new Expression("Remove" , @"!" , so_Remove, so_Check)
        };

        /// <summary>
        /// Single operator - remove single character
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expressionContent"></param>
        /// <returns></returns>
        public static string so_Remove(string value, string expressionContent)
        {
            string returnValue = "";
            foreach (char c in value)
            {
                if (c.ToString() != expressionContent)
                    returnValue += c;
            }

            return returnValue;
        }
        /// <summary>
        /// Each operator might contain a check func
        /// Checks if its valid before checking if it can be applied
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool so_Check(string value)
        {
            return value.Length > 2 ? false : true;
        }

        /// <summary>
        /// checks if the value string is a valid operator
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static (bool, Expression) IsOperator(string value)
        {
            Expression ex = FindByType(value.ToString(), ExpressionUseType.Tag);
            return ex != null ? (true, ex): (false, null);
        }

        /// <summary>
        /// Find a Expression using a ExpressionUseType
        /// </summary>
        /// <param name="value"></param>
        /// <param name="useType"></param>
        /// <returns></returns>
        private static Expression FindByType(string value, ExpressionUseType useType = ExpressionUseType.Tag)
        {
            foreach (var ex in simpleOps)
            {
                if (useType == ExpressionUseType.Name && ex.name == value)
                    return ex;
                if (useType == ExpressionUseType.Tag && ex.tag == value)
                    return ex;
            }
            return null;
        }

        /// <summary>
        /// Returns a list of Expressions that are found in the pattern string
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="extraCharacters"></param>
        /// <returns></returns>
        private static List<Expression> FindInString(string pattern, int extraCharacters = 0)
        {
            List<Expression> expressions = new List<Expression>();
            for (int i = 0; i < pattern.Length; i++)
            {
                int e = i + extraCharacters;

                if (e >= pattern.Length)
                    return expressions;

                (bool, Expression) result = IsOperator(pattern[e].ToString());

                if (result.Item1 == true)
                {
                    result.Item2.content = 
                        GetContentOfExpression(
                            pattern, e, 
                            FindEndOfExpression(
                                pattern, e));

                    if (result.Item2.check(result.Item2.content) == true)
                    {
                        expressions.Add(result.Item2);
                        continue;
                    }
                    else
                    {
                        expressions.AddRange(FindInString(pattern, extraCharacters + 1)); <--- something wrong here 
                    }
                }
                else
                {
                    expressions.AddRange(FindInString(pattern, extraCharacters + 1));
                }
            }
            return expressions;
        }

        /// <summary>
        /// Returns the index of the end of the current expression
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static int FindEndOfExpression(string pattern, int startIndex)
        {
            if (startIndex+1 >= pattern.Length)
                return -1;

            for (int i = startIndex+1; i < pattern.Length; i++)
            {
                if (IsOperator(pattern[i].ToString()).Item1 == true)
                {
                    return i-1;
                }
            }

            return pattern.Length-1;
        }

        /// <summary>
        /// Checked version of Substring
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private static string GetContentOfExpression(string pattern, int start, int end)
        {
            if (end == -1)
                end = pattern.Length;
            return pattern.Substring(start+1, end - start);
        }
    }
}
