using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using System;
using System.Data;

namespace BPO_calc.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {   
        public static double EvaluateExpression(string expression) // Первая функция, в которую попадает выражение
    {
        try
        {
            expression = expression.Replace(" ", "");
            if (Regex.IsMatch(expression, @"[^0-9+\-*/().^]"))
            {
                throw new ArgumentException("The expression contains invalid characters.");
            }

            expression = HandleParentheses(expression);
            expression = HandleExponentiation(expression);

            double result = EvaluateMathExpression(expression);

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error evaluating the expression: {ex.Message}");
        }
    }
    private static string HandleParentheses(string expression) // Функция для обработки выражений в скобках
    {
        while (expression.Contains("("))
        {
            int startIdx = expression.LastIndexOf("(");
            int endIdx = expression.IndexOf(")", startIdx);

            string subExpression = expression.Substring(startIdx + 1, endIdx - startIdx - 1).Trim();

            double subResult = EvaluateMathExpression(subExpression);

            expression = expression.Substring(0, startIdx) + subResult.ToString() + expression.Substring(endIdx + 1);
        }

        return expression;
    }

    private static string HandleExponentiation(string expression) // функция для обработки выражений со степенью
    {
        while (expression.Contains("^"))
        {

            int lastIndex = expression.LastIndexOf("^");

            string leftOperand, rightOperand;
            ExtractOperands(expression, lastIndex, out leftOperand, out rightOperand);

            if (string.IsNullOrWhiteSpace(leftOperand))
            {
                throw new InvalidOperationException("Left operand of '^' is missing or invalid.");
            }

            if (string.IsNullOrWhiteSpace(rightOperand))
            {
                throw new InvalidOperationException("Right operand of '^' is missing or invalid.");
            }

            double leftValue = Convert.ToDouble(leftOperand);
            double rightValue = EvaluateMathExpression(rightOperand);

            double result = Math.Pow(leftValue, rightValue);

            expression = expression.Substring(0, lastIndex - leftOperand.Length) + result.ToString() + expression.Substring(lastIndex + rightOperand.Length + 1);
        }

        return expression;
    }
        private static void ExtractOperands(string expression, int lastIndex, out string leftOperand, out string rightOperand) // Выражения слева и справа от степени
        {
            int leftStart = FindLeftOperandStart(expression, lastIndex);
            leftOperand = expression.Substring(leftStart, lastIndex - leftStart).Trim();

            int rightEnd = FindRightOperandEnd(expression, lastIndex);
            rightOperand = expression.Substring(lastIndex + 1, rightEnd - (lastIndex + 1)).Trim();
        }
        private static int FindLeftOperandStart(string expression, int lastIndex) // ищем первое выражение
        {
            int leftStart = lastIndex - 1;
            while (leftStart >= 0 && (char.IsDigit(expression[leftStart]) || expression[leftStart] == '.' || expression[leftStart] == ')'))
            {
                if (expression[leftStart] == '(')
                {
                    return leftStart; 
                }
                leftStart--;
            }
            return leftStart + 1;  
        }

        private static int FindRightOperandEnd(string expression, int lastIndex) // Ищем правое выражение
        {
            int rightEnd = lastIndex + 1;
            while (rightEnd < expression.Length && (char.IsDigit(expression[rightEnd]) || expression[rightEnd] == '.' || expression[rightEnd] == '('))
            {
                if (expression[rightEnd] == ')')
                {
                    return rightEnd;  
                }
                rightEnd++;
            }
            return rightEnd;  
        }

        private static double EvaluateMathExpression(string expression) // Метод для вычисления выражений(без знаков степени)
        {
            try
            {
                var result = new DataTable().Compute(expression, null);
                return Convert.ToDouble(result);
            }
            catch (Exception)
            {
                throw new InvalidOperationException("Error evaluating the expression.");
            }
        }
        [HttpGet("calc")]
        public IActionResult CalculateExpression(string expression)
        {
            try
            {
                double result = EvaluateExpression(expression);
                return Ok(result);  
            }
            catch (Exception ex)
            {
                return BadRequest($"Error evaluating the expression: {ex.Message}");
            }
        }
    }
}