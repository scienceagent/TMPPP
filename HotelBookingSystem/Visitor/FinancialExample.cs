using System;
using System.Collections.Generic;

namespace HotelBookingSystem.Visitor
{
    // ─── VISITOR INTERFACE ───────────────────────────────────────────────
    public interface IFinancialVisitor
    {
        void Visit(DepositAccount deposit);
        void Visit(LoanAccount    loan);
        void Visit(StockPortfolio stock);
    }

    // ─── ELEMENT INTERFACE ───────────────────────────────────────────────
    public interface IFinancialInstrument
    {
        void Accept(IFinancialVisitor visitor);
        string Name { get; }
    }

    // ─── CONCRETE ELEMENTS ───────────────────────────────────────────────
    public class DepositAccount : IFinancialInstrument
    {
        public string  Name        { get; }
        public decimal Balance     { get; }
        public decimal InterestRate { get; }  // % anual
        public int     DaysRemaining { get; }

        public DepositAccount(string name, decimal balance, decimal rate, int days)
        { Name = name; Balance = balance; InterestRate = rate; DaysRemaining = days; }

        public void Accept(IFinancialVisitor visitor) => visitor.Visit(this);
    }

    public class LoanAccount : IFinancialInstrument
    {
        public string  Name          { get; }
        public decimal OutstandingBalance { get; }
        public decimal InterestRate  { get; }
        public int     RiskScore     { get; }  // 1-10, 10 = risc maxim

        public LoanAccount(string name, decimal balance, decimal rate, int risk)
        { Name = name; OutstandingBalance = balance; InterestRate = rate; RiskScore = risk; }

        public void Accept(IFinancialVisitor visitor) => visitor.Visit(this);
    }

    public class StockPortfolio : IFinancialInstrument
    {
        public string  Name        { get; }
        public decimal MarketValue { get; }
        public decimal Volatility  { get; }  // % volatilitate anuala

        public StockPortfolio(string name, decimal value, decimal vol)
        { Name = name; MarketValue = value; Volatility = vol; }

        public void Accept(IFinancialVisitor visitor) => visitor.Visit(this);
    }

    // ─── VISITOR 1: Calcul Risc ───────────────────────────────────────────
    public class RiskCalculatorVisitor : IFinancialVisitor
    {
        private decimal _totalRiskExposure;
        private readonly List<string> _riskReport = new();

        public void Visit(DepositAccount deposit)
        {
            decimal risk = deposit.Balance > 100_000m ? deposit.Balance * 0.01m : 0;
            _totalRiskExposure += risk;
            _riskReport.Add($"Depozit [{deposit.Name}]: risc={risk:C} (garantat FGDS)");
        }

        public void Visit(LoanAccount loan)
        {
            decimal risk = loan.OutstandingBalance * (loan.RiskScore / 10m);
            _totalRiskExposure += risk;
            _riskReport.Add($"Credit [{loan.Name}]: risc={risk:C} (scor risc={loan.RiskScore}/10)");
        }

        public void Visit(StockPortfolio stock)
        {
            decimal risk = stock.MarketValue * (stock.Volatility / 100m);
            _totalRiskExposure += risk;
            _riskReport.Add($"Actiuni [{stock.Name}]: risc={risk:C} (vol={stock.Volatility}%)");
        }

        public void PrintReport()
        {
            Console.WriteLine("\n=== RAPORT RISC ===");
            _riskReport.ForEach(r => Console.WriteLine($"  {r}"));
            Console.WriteLine($"  TOTAL EXPUNERE LA RISC: {_totalRiskExposure:C}");
        }
    }

    // ─── VISITOR 2: Raport Fiscal ─────────────────────────────────────────
    public class TaxReportVisitor : IFinancialVisitor
    {
        private decimal _totalTaxableIncome;
        private const decimal TaxRate = 0.18m; // 18% impozit pe venit

        public void Visit(DepositAccount deposit)
        {
            decimal interest = deposit.Balance * deposit.InterestRate / 100m
                             * deposit.DaysRemaining / 365m;
            decimal tax      = interest * TaxRate;
            _totalTaxableIncome += tax;
            Console.WriteLine($"  Depozit [{deposit.Name}]: dobanzi={interest:C} | impozit={tax:C}");
        }

        public void Visit(LoanAccount loan)
        {
            Console.WriteLine($"  Credit [{loan.Name}]: neimpozabil (deducere dobanda)");
        }

        public void Visit(StockPortfolio stock)
        {
            decimal estimatedGain = stock.MarketValue * 0.10m;
            decimal tax           = estimatedGain * TaxRate;
            _totalTaxableIncome += tax;
            Console.WriteLine($"  Actiuni [{stock.Name}]: castig={estimatedGain:C} | impozit={tax:C}");
        }

        public void PrintSummary()
            => Console.WriteLine($"  TOTAL IMPOZIT DE PLATIT: {_totalTaxableIncome:C}");
    }
}
