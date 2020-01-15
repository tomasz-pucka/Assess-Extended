﻿namespace CalculationsEngine.Assess.Assess
{
    public struct CriterionCoefficient
    {
        public string CriterionName;
        public double Coefficient;

        public CriterionCoefficient(string criterionName, double coefficient)
        {
            CriterionName = criterionName;
            Coefficient = coefficient;
        }
    }
}