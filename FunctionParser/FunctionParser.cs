using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FunctionParser
{
    public class Commons
    {
        internal static string[] SplitComma(string input)
        {
            string pattern1 = @"\(.*?\)";

            foreach (Match m in Regex.Matches(input, pattern1))
            {
                input = input.Replace(m.Groups[0].ToString(),
                                      m.Groups[0].ToString().Replace(",", "___86DAF157-DDD8-4250-80EF-623004AD422F___"));
            }

            string[] result = input.Split(',');
            for (int i = 0; i < result.Count(); i++)
            {
                result[i] = result[i].Replace("___86DAF157-DDD8-4250-80EF-623004AD422F___", ",").Trim();

            }
            return result;
        }
        internal static string KeepOnlyQuotedSpaces(string input)
        {
            string pattern1 = @""".*?""";

            foreach (Match m in Regex.Matches(input, pattern1))
            {
                input = input.Replace(m.Groups[0].ToString(),
                                      m.Groups[0].ToString().Replace(" ", "___9D4BC392-BDD6-48F9-8204-4C64E0464882___"));
            }

            input = input.Replace(" ", "");
            input = input.Replace("___9D4BC392-BDD6-48F9-8204-4C64E0464882___", " ");
            return input;
        }
    }

    public class Statics
    {
        internal static bool IsFactor(ConnectorsClass Connectors, string factor)
        {
            double tst;
            if (double.TryParse(factor, out tst))
                return true;
            else if (factor.StartsWith("\"") && factor.EndsWith("\"") && factor.Length > 1)
                return true;
            else if (factor.StartsWith("(") && factor.EndsWith(")") && Statics.IsExpression(Connectors, factor.Substring(1, factor.Length - 2)))
                return true;
            else if (factor.StartsWith("-") && Statics.IsFactor(Connectors, factor.Substring(1, factor.Length - 1)))
                return true;
            else if (FunctionParser.Function.IsFunction(Connectors, factor))
                return true;
            else if (IsID(Connectors, factor))
                return true;
            else { return false; }
        }

        internal static bool IsID(ConnectorsClass Connectors, string id)
        {
           return  Connectors.IsProperId(id);    // always true, but cannot be whitespace !
         
        }

        internal static bool IsTerm(ConnectorsClass Connectors, string term)
        {
            int oprIndx = -1;
            int brackets = 0;
            for (int i = term.Length - 1; i > 0; i--)
            {
                if ((term[i] == '*' || term[i] == '/' || term[i] == '^') && (brackets == 0))
                {
                    oprIndx = i;
                    break;
                }
                else if (term[i] == ')') brackets++;
                else if (term[i] == '(') brackets--;
            }
            if (oprIndx > 0)
            {

                string subterm, factor;
                subterm = term.Substring(0, oprIndx);
                factor = term.Substring(oprIndx + 1);
                return Statics.IsTerm(Connectors, subterm) && Statics.IsFactor(Connectors,factor);
            }
            else
            {
                return FunctionParser.Statics.IsFactor(Connectors, term);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="ids">if ids is null, we won't check that aspect and validate anything as expression concerning ids</param>
        /// <returns></returns>
        public static bool IsExpression(ConnectorsClass Connectors, string expr, string[] ids = null)
        {
            expr = expr.Replace(" ", "");
            int oprIndx = -1;
            int brackets = 0;
            for (int i = expr.Length - 1; i > 0; i--)
            {
                if (((expr[i] == '-' && !IsOperator(expr[i - 1]))
                    || expr[i] == '+') && (brackets == 0))
                {
                    oprIndx = i;
                    break;
                }
                else if (expr[i] == ')') brackets++;
                else if (expr[i] == '(') brackets--;
            }
            if (oprIndx > 0)
            {
                string subExpr, term;
                subExpr = expr.Substring(0, oprIndx);
                term = expr.Substring(oprIndx + 1);
                return (FunctionParser.Statics.IsTerm(Connectors, term) && IsExpression(Connectors, subExpr));
            }
            else
            {
                return FunctionParser.Statics.IsTerm(Connectors, expr);
            }
        }
        internal static bool IsOperator(char c)
        {
            return c == '-' || c == '+' || c == '*' || c == '/' || c == '^';
        }


    }

        public  class ConnectorsClass
    {
        public  Func<string, bool> IsProperId = a => true;
        public  Func<string, object> GetIdValue = a => 0.0;
        public  Func<string, object[], object> ExecuteFunction = (a, b) => 0.0;
    }

    public class Function : ParsTreeNode
    {
        //ConnectorsClass Connectors = new ConnectorsClass();
        //public class Executor
        //{
        //    public Func<object, object> Compute;
        //    public Func<object, object, object> Compute2;
        //}

        //public static Dictionary<string, Executor> _Catalog = new Dictionary<string, Executor>();

        //public enum FunctionEnum
        //{
        //    Sinh, Sin, Cosh, Cos, Tanh, Tan, Coth, Cot, Sich, Sic, Csch, Csc, E, Log, Ln
        //}

        // an alpha character with alphanumerics after, at start of with a whitespace before
        public static Regex fnRecognizer = new Regex(
      "(^|\\W)(?<rad>\\p{L}+[\\w\\d_]*)\\((?<args>.*)\\)",
    RegexOptions.CultureInvariant
    | RegexOptions.Compiled
    );





        public static bool IsFunction(ConnectorsClass Connectors, string function)
        {
            function = function.ToLower();
            Match data = fnRecognizer.Match(function);

            if (data.Success)
            {
                string attributes = data.Groups["args"].Value;

                if (string.IsNullOrWhiteSpace(attributes)) return true; //no args

                return Commons.SplitComma(attributes).All(a => Statics.IsTerm(Connectors,a));
            }
            
            return false;
        }
        public string FuncName;
      //  public FunctionEnum Func { get; set; }
        public List<Term> Terms = new List<Term>();

        static Function()
        {
            //_Catalog.Add("do", new Executor() {  Compute = a => (double)a * 2 , Compute2 = (a,b)=> (double)a + (double)b });
        }

        public Function(ConnectorsClass Connectors, string function,  ParsTreeNode parent)
            : base(function, parent)
        {

            Match data = fnRecognizer.Match(function);

            // foreach (string func in _Catalog.Keys.OrderByDescending(a => a.Length))//Enum.GetNames(typeof(FunctionEnum)))
            {
              //  if (function.ToLower().StartsWith(func.ToLower()))
              if (data.Success)
                {

                    //  Func = _Catalog[func];// (FunctionEnum)Enum.Parse(typeof(FunctionEnum), func);
                    FuncName = data.Groups["rad"].Value;

                    //if (function.Length >= func.Length + 2) // enough to have 2 parenthesis
                    {
                        string attributes = data.Groups["args"].Value;// function.Substring(func.Length + 1, function.Length - func.Length - 2);

                        if (!string.IsNullOrWhiteSpace(attributes))
                        {
                            foreach (var el in Commons.SplitComma(attributes))
                            {
                                this.Terms.Add(new Term(Connectors, el, this));
                            }
                        }
                    }


                    //break;
                }
            }
        }
        public override object CalculateValue(ConnectorsClass Connectors)
        {
            object[] termValue = this.Terms.Select(a => a.CalculateValue(Connectors)).ToArray();

            return Connectors.ExecuteFunction(FuncName, termValue);

            //switch (termValue.Length)
            //{
            //    case 1:
            //        default:
            //        return Func.Compute(termValue[0]);
            //    case 2:
            //        return Func.Compute2(termValue[0], termValue[1]);
            //}

                    /*switch (Func)
                    {
                        case FunctionEnum.Sin:
                            ret = Math.Sin(termValue * Math.PI / 180);
                            break;
                        case FunctionEnum.Cos:
                            ret = Math.Sin(termValue * Math.PI / 180);
                            break;
                        case FunctionEnum.Tan:
                            ret = Math.Tan(termValue * Math.PI / 180);
                            break;
                        case FunctionEnum.Sinh:
                            ret = Math.Sinh(termValue * Math.PI / 180);
                            break;
                        case FunctionEnum.Cosh:
                            ret = Math.Cosh(termValue * Math.PI / 180);
                            break;
                        case FunctionEnum.Tanh:
                            ret = Math.Tanh(termValue * Math.PI / 180);
                            break;
                        case FunctionEnum.Csc:
                            ret = (1 / Math.Sin(termValue * Math.PI / 180));
                            break;
                        case FunctionEnum.Sic:
                            ret = (1 / Math.Cos(termValue * Math.PI / 180));
                            break;
                        case FunctionEnum.Cot:
                            ret = (1 / Math.Tan(termValue * Math.PI / 180));
                            break;
                        case FunctionEnum.Csch:
                            ret = (1 / Math.Sinh(termValue * Math.PI / 180));
                            break;
                        case FunctionEnum.Sich:
                            ret = (1 / Math.Cosh(termValue * Math.PI / 180));
                            break;
                        case FunctionEnum.Coth:
                            ret = (1 / Math.Tanh(termValue * Math.PI / 180));
                            break;
                        case FunctionEnum.E:
                            ret = (Math.Exp(termValue));
                            break;
                        case FunctionEnum.Log:
                            ret = (Math.Log10(termValue));
                            break;
                        case FunctionEnum.Ln:
                            ret = (Math.Log(termValue, Math.E));
                            break;
                    }
                    return ret;*/

            }
        public override object[] CalculateValue(ConnectorsClass Connectors,  object[][] idsValues)
        {
            return idsValues.Select(a => CalculateValue(Connectors)).ToArray();

       /*     object[][] termValue = this.Terms.Select (a=> a.CalculateValue(ids, idsValues)).ToArray();
            object[] ret = new object[termValue.Length];
            
            for (int i = 0; i < ret.Length; i++)
                ret[i] = Math.Sin(termValue[i][0] * Math.PI / 180);*/

           // return termValue.e Func.Compute(termValue[0]);

            /* switch (Func)
             {
                 case FunctionEnum.Sin:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = Math.Sin(termValue[i] * Math.PI / 180);
                     break;
                 case FunctionEnum.Cos:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = Math.Cos(termValue[i] * Math.PI / 180);
                     break;
                 case FunctionEnum.Tan:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = Math.Tanh(termValue[i] * Math.PI / 180);
                     break;
                 case FunctionEnum.Sinh:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = Math.Sinh(termValue[i] * Math.PI / 180);
                     break;
                 case FunctionEnum.Cosh:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = Math.Cosh(termValue[i] * Math.PI / 180);
                     break;
                 case FunctionEnum.Tanh:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = Math.Tanh(termValue[i] * Math.PI / 180);
                     break;
                 case FunctionEnum.Csc:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (1 / Math.Sin(termValue[i] * Math.PI / 180));
                     break;
                 case FunctionEnum.Sic:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (1 / Math.Cos(termValue[i] * Math.PI / 180));
                     break;
                 case FunctionEnum.Cot:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (1 / Math.Tan(termValue[i] * Math.PI / 180));
                     break;
                 case FunctionEnum.Csch:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (1 / Math.Sinh(termValue[i] * Math.PI / 180));
                     break;
                 case FunctionEnum.Sich:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (1 / Math.Cosh(termValue[i] * Math.PI / 180));
                     break;
                 case FunctionEnum.Coth:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (1 / Math.Tanh(termValue[i] * Math.PI / 180));
                     break;
                 case FunctionEnum.E:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (Math.Exp(termValue[i]));
                     break;
                 case FunctionEnum.Log:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (Math.Log10(termValue[i]));
                     break;
                 case FunctionEnum.Ln:
                     for (int i = 0; i < ret.Length; i++)
                         ret[i] = (Math.Log(Math.E, termValue[i]));
                     break;
             }*/
        //    return ret;

        }
    }
    public class Factor : ParsTreeNode
    {
        public enum FactorExpansion
        {
            Number,//1,2,3,etc
            String,//"abc"
            Function,//sin,cos,etc
            MinuFactor,//-x,-15,-sin,-(x+1),etc
            WrappedExpression,//(expression)
            ID//x
        }
       
        public FactorExpansion Expansion { get; set; }
        public Function Function { get; set; }
        public Expression WrappedExpression { get; set; }
        public Factor InnerFactor;
        public Factor(ConnectorsClass Connectors, string factor, ParsTreeNode parent)
            : base(factor, parent)
        {

            this.Value = factor;
            double value;
            if (double.TryParse(factor, out value))
            {
                this.Expansion = FactorExpansion.Number;
            }
            else
            {
                if (factor.StartsWith("(") && factor.EndsWith(")"))
                {
                    this.Expansion = FactorExpansion.WrappedExpression;
                    this.WrappedExpression = new Expression(Connectors, factor.Substring(1, factor.Length - 2), this);
                }
                else if (factor.StartsWith("\"") && factor.EndsWith("\""))
                {
                    //this.Expansion = FactorExpansion.WrappedExpression;
                    this.Expansion = FactorExpansion.String;
                    this.Value = factor.Substring(1, factor.Length - 2);
                }
                else if (Function.IsFunction(Connectors, factor))
                {
                    this.Expansion = FactorExpansion.Function;
                    this.Function = new Function(Connectors, factor, this);

                }
                else if(factor.StartsWith("-"))
                {
                    this.Expansion = FactorExpansion.MinuFactor;
                    this.InnerFactor = new Factor(Connectors, factor.Substring(1, factor.Length - 1), this);
                }
                else
                {
                    this.Expansion = FactorExpansion.ID;
                }
            }

        }
        public override object CalculateValue(ConnectorsClass Connectors)
        {
            if (Expansion == FactorExpansion.Number)
            {
                return (double.Parse(this.Value));
            }
            if (Expansion == FactorExpansion.String)
            {
                return this.Value;
            }
            else if (Expansion == FactorExpansion.WrappedExpression)
            {
                return (WrappedExpression.CalculateValue(Connectors));
            }
            else if (Expansion == FactorExpansion.Function)
            {
                return (this.Function.CalculateValue(Connectors));
            }
            else if(Expansion== FactorExpansion.MinuFactor)
            {
                return -(double)(this.InnerFactor.CalculateValue(Connectors));
            }
            else
            {
                return Connectors.GetIdValue(this.Value);
              //  return 2.0;
                //ID
            /*    int idIndex = -1;
                for (int i = 0; i < IDs.Length; i++)
                    if (IDs[i] == this.Value)
                    {
                        idIndex = i;
                        break;
                    }
                return idValues[idIndex];*/

            }
        }
        public override object[] CalculateValue(ConnectorsClass Connectors,  object[][] idsValues)
        {
            //IEnumerator idE = ids.GetEnumerator();
            //IEnumerator idsValuesE = idsValues.GetEnumerator();

            return idsValues.Select(a => CalculateValue(Connectors)).ToArray();
        }
    }
    public class Term : ParsTreeNode
    {
        public enum TermExpansion
        {
            TermMulFactor,
            TermDivFactor,
            TermPowFactor,
            Factor
        }
        public TermExpansion Expansion { get; set; }
        public Term SubTerm { get; set; }
        public Factor Factor { get; set; }
        public Term(ConnectorsClass Connectors, string term, ParsTreeNode parent)
            : base(term, parent)
        {

            this.Value = term;
            int oprIndx = -1;
            int brackets = 0;
            for (int i = term.Length - 1; i > 0; i--)
            {
                if ((term[i] == '*' || term[i] == '/' || term[i] == '^') && (brackets == 0))
                {
                    oprIndx = i;
                    break;
                }
                else if (term[i] == ')') brackets++;
                else if (term[i] == '(') brackets--;
            }
            if (oprIndx > 0)
            {

                string subterm, factor;
                char opr = term[oprIndx];
                subterm = term.Substring(0, oprIndx);
                factor = term.Substring(oprIndx + 1);
                this.Factor = new Factor(Connectors, factor,this);
                this.SubTerm = new Term(Connectors, subterm,this);
                if (opr == '*')
                {
                    this.Expansion = TermExpansion.TermMulFactor;
                }
                else if (opr == '/')
                {
                    this.Expansion = TermExpansion.TermDivFactor;
                }
                else
                {
                    this.Expansion = TermExpansion.TermPowFactor;
                }
            }
            else
            {
                this.Expansion = TermExpansion.Factor;
                this.Factor = new Factor(Connectors, term, this);
            }

        }
        public override object CalculateValue(ConnectorsClass Connectors)
        {
            //rlel will crash if not numbers (doubles)
            if (Expansion == TermExpansion.TermDivFactor)
            {
                return ((double)this.SubTerm.CalculateValue(Connectors) / (double)this.Factor.CalculateValue(Connectors));
            }
            else if (Expansion == TermExpansion.TermMulFactor)
            {
                return ((double)this.SubTerm.CalculateValue(Connectors) * (double)this.Factor.CalculateValue(Connectors));
            }
            else if (Expansion == TermExpansion.TermPowFactor)
            {
                return (Math.Pow((double)this.SubTerm.CalculateValue(Connectors), (double)this.Factor.CalculateValue(Connectors)));
            }
            else
                return (this.Factor.CalculateValue(Connectors));
        }
        public override object[] CalculateValue(ConnectorsClass Connectors,  object[][] idsValues)
        {
            return idsValues.Select(a => CalculateValue(Connectors)).ToArray();
            /*
            object[] ret = new object[idsValues.Length];
            if (Expansion == TermExpansion.TermDivFactor)
            {
                object[] subtermValues = this.SubTerm.CalculateValue(ids, idsValues);
                object[] factorValues = this.Factor.CalculateValue(ids, idsValues);
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = subtermValues[i] / factorValues[i];
                return ret;
            }
            else if (Expansion == TermExpansion.TermMulFactor)
            {
                object[] subtermValues = this.SubTerm.CalculateValue(ids, idsValues);
                object[] factorValues = this.Factor.CalculateValue(ids, idsValues);
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = subtermValues[i] * factorValues[i];
                return ret;
            }
            else if (Expansion == TermExpansion.TermPowFactor)
            {
                object[] subtermValues = this.SubTerm.CalculateValue(ids, idsValues);
                object[] factorValues = this.Factor.CalculateValue(ids, idsValues);
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = Math.Pow(subtermValues[i], factorValues[i]);
                return ret;
            }
            else
                return (this.Factor.CalculateValue(ids, idsValues));*/
        }
    }
    public class Expression : ParsTreeNode
    {
        public enum ExpressionExpansion
        {
            ExpressionPlusTerm,
            ExpressionMinusTerm,
            Term
        }


        public static Regex matcher = new Regex(
      "\\\"[^\\\"]+\\\"|(.+)",
    RegexOptions.CultureInvariant
    | RegexOptions.Compiled
    );


        public static string Compute(string text, ConnectorsClass Connectors)
        {
            if (Statics.IsExpression(Connectors, text/*, idsNames*/))
            {

                Expression expression = new Expression(Connectors, text, null);
                //TreeNode expr = new TreeNode(txt_expr.Text);
                //CreateTreeNodes(expression, expr);
                //trvw_ParseTree.Nodes.Clear();
                //trvw_ParseTree.Nodes.Add(expr);
                //trvw_ParseTree.ExpandAll();
                //expression.ad
                string result = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}", expression.CalculateValue(Connectors));
               // string.Format("{0}", System.Globalization.CultureInfo.InvariantCulture);
                return result;
            }
            else
            {
                return null;
            }
        }



        public ExpressionExpansion Expansion { get; set; }
        public Term Term { get; set; }
        public Expression SubExpression { get; set; }
        public Expression(ConnectorsClass Connectors, string expr, ParsTreeNode parent)
            : base(expr, parent)
        {


            expr= Commons.KeepOnlyQuotedSpaces(expr);

            int oprIndx = -1;
            int brackets = 0;
            for (int i = expr.Length - 1; i > 0; i--)
            {
                if (((expr[i] == '-' && !Statics.IsOperator(expr[i - 1]))
                    || expr[i] == '+') && (brackets == 0))
                {
                    oprIndx = i;
                    break;
                }
                else if (expr[i] == ')') brackets++;
                else if (expr[i] == '(') brackets--;
            }
            if (oprIndx > 0)
            {
                string subExpr, term;
                char opr;
                subExpr = expr.Substring(0, oprIndx);
                term = expr.Substring(oprIndx + 1);
                opr = expr[oprIndx];
                this.Term = new Term(Connectors, term, this);
                this.SubExpression = new Expression(Connectors, subExpr, this);
                if (opr == '-')
                {
                    Expansion = ExpressionExpansion.ExpressionMinusTerm;
                }
                else
                {
                    Expansion = ExpressionExpansion.ExpressionPlusTerm;
                }
            }
            else
            {
                Expansion = ExpressionExpansion.Term;
                this.Term = new Term(Connectors, expr, this);
            }
        }
        public override object CalculateValue(ConnectorsClass Connectors)
        {
            if (Expansion == ExpressionExpansion.ExpressionMinusTerm)
            {
                var a = this.SubExpression.CalculateValue(Connectors);
                var b = this.Term.CalculateValue(Connectors);

                if (a is double && b is double)
                    return ((double)a - (double)b);
                else
                    return a.ToString();    //IMPOSSIBLE
            }
            else if (Expansion == ExpressionExpansion.ExpressionPlusTerm)
            {
                var a = this.SubExpression.CalculateValue(Connectors);
                var b = this.Term.CalculateValue(Connectors);
                if (a is double && b is double)
                    return ((double)a + (double)b);
                else
                    return a.ToString() + b.ToString();
            }
            else
                return (this.Term.CalculateValue(Connectors));
        }
        public override object[] CalculateValue(ConnectorsClass Connectors,  object[][] idsValues)
        {
            return idsValues.Select(a => CalculateValue(Connectors)).ToArray();
            /*
            object[] ret = new object[idsValues.Length];
            if (Expansion == ExpressionExpansion.ExpressionMinusTerm)
            {
                object[] subExprValues = this.SubExpression.CalculateValue(ids, idsValues);
                object[] termValues = this.Term.CalculateValue(ids, idsValues);
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = subExprValues[i] - termValues[i];
                return ret;
            }
            else if (Expansion == ExpressionExpansion.ExpressionPlusTerm)
            {
                object[] subExprValues = this.SubExpression.CalculateValue(ids, idsValues);
                object[] termValues = this.Term.CalculateValue(ids, idsValues);
                for (int i = 0; i < ret.Length; i++)
                    ret[i] = subExprValues[i] + termValues[i];
                return ret;
            }
            else
                return (this.Term.CalculateValue(ids, idsValues));*/
        }

    }
    public abstract class ParsTreeNode
    {
        public string Value { get; set; }
        public abstract object CalculateValue(ConnectorsClass Connectors);
        public abstract object[] CalculateValue(ConnectorsClass Connectors,  object[][] idsValues);
        public ParsTreeNode Parent { get; set; }
        //public string[] IDs { get; set; }
        protected ParsTreeNode(string value, ParsTreeNode parnet)
        {
            this.Value = value.Replace(" ","");
            this.Parent = parnet;
           // this.IDs = ids;
        }
    }
}
