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
            // Find the innermost parentheses
            int startIdx = expression.LastIndexOf("(");
            int endIdx = expression.IndexOf(")", startIdx);

            // Extract the sub-expression inside the parentheses
            string subExpression = expression.Substring(startIdx + 1, endIdx - startIdx - 1).Trim();

            // Evaluate the sub-expression recursively
            double subResult = EvaluateMathExpression(subExpression);

            // Replace the parentheses with the result
            expression = expression.Substring(0, startIdx) + subResult.ToString() + expression.Substring(endIdx + 1);
        }

        return expression;
    }

    private static string HandleExponentiation(string expression) // функция для обработки выражений со степенью
    {
        while (expression.Contains("^"))
        {
            // Find the last occurrence of '^' (rightmost one)
            int lastIndex = expression.LastIndexOf("^");

            // Extract two operands around the '^'
            string leftOperand, rightOperand;
            ExtractOperands(expression, lastIndex, out leftOperand, out rightOperand);

            // Check if there is a valid expression on the left
            if (string.IsNullOrWhiteSpace(leftOperand))
            {
                throw new InvalidOperationException("Left operand of '^' is missing or invalid.");
            }

            // Check if the right operand is valid
            if (string.IsNullOrWhiteSpace(rightOperand))
            {
                throw new InvalidOperationException("Right operand of '^' is missing or invalid.");
            }

            // Evaluate the left and right operands to get the numeric values
            double leftValue = Convert.ToDouble(leftOperand);
            double rightValue = EvaluateMathExpression(rightOperand);

            // Compute the result of leftOperand ^ rightOperand
            double result = Math.Pow(leftValue, rightValue);

            // Replace the current '^' operation in the expression with the result
            expression = expression.Substring(0, lastIndex - leftOperand.Length) + result.ToString() + expression.Substring(lastIndex + rightOperand.Length + 1);
        }

        return expression;
    }
        private static void ExtractOperands(string expression, int lastIndex, out string leftOperand, out string rightOperand) // Выражения слева и справа от степени
        {
            // Find the left operand (from the position before the operator, backward)
            int leftStart = FindLeftOperandStart(expression, lastIndex);
            leftOperand = expression.Substring(leftStart, lastIndex - leftStart).Trim();

            // Find the right operand (from the position after the operator, forward)
            int rightEnd = FindRightOperandEnd(expression, lastIndex);
            rightOperand = expression.Substring(lastIndex + 1, rightEnd - (lastIndex + 1)).Trim();
        }
        private static int FindLeftOperandStart(string expression, int lastIndex) // ищем первое выражение
        {
            int leftStart = lastIndex - 1;
            // Look backward for an operator or open parenthesis
            while (leftStart >= 0 && (char.IsDigit(expression[leftStart]) || expression[leftStart] == '.' || expression[leftStart] == ')'))
            {
                if (expression[leftStart] == '(')
                {
                    return leftStart; // We hit a parenthesis, stop
                }
                leftStart--;
            }
            return leftStart + 1;  // Adjust the index to start from the number
        }

        private static int FindRightOperandEnd(string expression, int lastIndex) // Ищем правое выражение
        {
            int rightEnd = lastIndex + 1;
            // Look forward for the next operator or closing parenthesis
            while (rightEnd < expression.Length && (char.IsDigit(expression[rightEnd]) || expression[rightEnd] == '.' || expression[rightEnd] == '('))
            {
                if (expression[rightEnd] == ')')
                {
                    return rightEnd;  // We hit a closing parenthesis, stop
                }
                rightEnd++;
            }
            return rightEnd;  // End the operand at the first operator or end of the string
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
                // Call the EvaluateExpression method to compute the result
                double result = EvaluateExpression(expression);
                return Ok(result);  // Return the result of the calculation
            }
            catch (Exception ex)
            {
                // Catch any exceptions and return an error message
                return BadRequest($"Error evaluating the expression: {ex.Message}");
            }
        }
    }
}