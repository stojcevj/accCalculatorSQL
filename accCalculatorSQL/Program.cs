using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace accCalculatorSQL
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DateTime dateFrom = new DateTime(2024, 3, 11);
            DateTime dateDue = new DateTime(2025, 3, 11);
            DateTime dateTo = new DateTime(2025, 3, 11);
            short interestMaturityType = 4;
            decimal? initialAmount = 1000000;
            decimal? interestPercent = 2.521m;
            short? calculationMethod = 1;
            short? interestPaymentType = 2;

            List<tbl2> result = accCalculator(dateFrom, dateDue, dateTo, interestMaturityType, initialAmount,
                                              interestPercent, calculationMethod, interestPaymentType);

            Console.WriteLine("Staj tuka breakpoint i a listata gore so e result prai i View");
        }

        static List<tbl2> accCalculator(DateTime DateFrom, DateTime DateDue, DateTime DateTo, short InterestMaturityType, decimal? InitialAmount,
            decimal? InterestPercent, short? CalculationMethod, short? InterestPaymentType ) {

            DateTime Date;
            string EoyDateFrom;
            DateTime tmpDate1;
            short RoundNumber = 2;
            decimal TotalDue = 0;
            DateTime Today = DateTime.Now;
            List<tbl1> tmpTbl = new List<tbl1>();
            List<tbl2> tmpTbl2 = new List<tbl2>();

            if (CalculationMethod == null) CalculationMethod = 4;
            if (InterestPaymentType == null) InterestPaymentType = 1;

            if (InitialAmount == null) InitialAmount = 0;
            if (InterestPercent == null) InterestPercent = 0;

            if (InitialAmount != 0) tmpTbl.Add(new tbl1 { DATE = DateFrom, Amount = InitialAmount, f_Amount = 1 });
            if (InterestPercent != 0) tmpTbl.Add(new tbl1 { DATE = DateFrom, InterestPercent = InterestPercent, f_Interest = 1 });

            if (DateDue < DateTo) DateTo = DateDue;

            Date = DateFrom;

            while (Date <= DateTo)
            {
                Date = new DateTime(Date.Year, Date.Month, DateTime.DaysInMonth(Date.Year, Date.Month));

                if (Date <= DateTo)
                    tmpTbl.Add(new tbl1 { DATE = Date, f_EOM = 1 });

                if(Date.Month == 12 && Date <= DateTo)
                    if (CalculationMethod == 1 || CalculationMethod == 4)
                        tmpTbl.Add(new tbl1 { DATE = Date, f_EOY = 1, DaysInYear = Date.DayOfYear});
                    else
                        tmpTbl.Add(new tbl1 { DATE = Date, f_EOY = 1, DaysInYear = 360 });

                Date = Date.AddDays(1);
            }

            int i = 1;
            int[] validTypes = new int[] { 2, 3, 4, 5, 6, 7, 9 };
            Date = DateFrom;

            while (validTypes.Contains(InterestMaturityType) && Date <= DateTo)
            {
                if (InterestMaturityType == 2) Date = DateFrom.AddDays(i);
                if (InterestMaturityType == 3) Date = DateFrom.AddDays(i*7);
                if (InterestMaturityType == 4) Date = DateFrom.AddMonths(i);
                if (InterestMaturityType == 5) Date = DateFrom.AddMonths(i*3);
                if (InterestMaturityType == 6) Date = DateFrom.AddMonths(i*6);
                if (InterestMaturityType == 7) Date = DateFrom.AddMonths(i*12);
                if (InterestMaturityType == 9)
                {
                    DateTime resultDate = DateFrom.AddMonths((i - 1) * 12); 

                    DateTime yearStart = new DateTime(resultDate.Year, 1, 1); 

                    DateTime finalDate = yearStart.AddYears(1).AddDays(-1);

                    Date = finalDate;
                }
                if (Date <= DateTo)
                {
                    tmpTbl.Add(new tbl1 { DATE = Date, f_DueDate = 1 });
                }

                i++;

                if (InterestMaturityType == 1 || InterestMaturityType == 8) Date = Date.AddDays(1);
            }

            if (DateDue <= DateTo) tmpTbl.Add(new tbl1 { DATE = DateDue, f_DueDate = 1 });

            DateTime pom = DateFrom;

            while (pom <= DateTo)
            {
                if (tmpTbl.Find(s => s.DATE == pom) == null)
                    tmpTbl.Add(new tbl1 { DATE = pom });

                pom = pom.AddDays(1);
            }

            tmpTbl.Add(new tbl1 { DATE = DateTo, f_CalcDate = 1 });

            Dictionary<DateTime, tbl1> groupedByDate = new Dictionary<DateTime, tbl1>();

            foreach (tbl1 tbl in tmpTbl)
            {
                if (groupedByDate.ContainsKey(tbl.DATE)) {
                    groupedByDate.TryGetValue(tbl.DATE, out var groupedToBeAdded);
                    groupedToBeAdded.Amount += tbl.Amount;
                    if (tbl.InterestPercent > groupedToBeAdded.InterestPercent || groupedToBeAdded.InterestPercent == null) groupedToBeAdded.InterestPercent = tbl.InterestPercent;
                    if (tbl.DaysInYear > groupedToBeAdded.DaysInYear) groupedToBeAdded.DaysInYear = tbl.DaysInYear;
                    groupedToBeAdded.f_EOM += tbl.f_EOM;
                    if (groupedToBeAdded.f_EOM > 0) groupedToBeAdded.f_EOM = 1; else groupedToBeAdded.f_EOM = 0;

                    groupedToBeAdded.f_DueDate += tbl.f_DueDate;
                    if (groupedToBeAdded.f_DueDate > 0) groupedToBeAdded.f_DueDate = 1; else groupedToBeAdded.f_DueDate = 0;

                    groupedToBeAdded.f_EOY += tbl.f_EOY;
                    if (groupedToBeAdded.f_EOY > 0) groupedToBeAdded.f_EOY = 1; else groupedToBeAdded.f_EOY = 0;

                    groupedToBeAdded.f_CalcDate += tbl.f_CalcDate;
                    if (groupedToBeAdded.f_CalcDate > 0) groupedToBeAdded.f_CalcDate = 1; else groupedToBeAdded.f_CalcDate = 0;

                    groupedToBeAdded.f_Amount += tbl.f_Amount;
                    if (groupedToBeAdded.f_Amount > 0) groupedToBeAdded.f_Amount = 1; else groupedToBeAdded.f_Amount = 0;

                    groupedToBeAdded.f_Interest += tbl.f_Interest;
                    if (groupedToBeAdded.f_Interest > 0) groupedToBeAdded.f_Interest = 1; else groupedToBeAdded.f_Interest = 0;
                    groupedByDate[tbl.DATE] = groupedToBeAdded;
                }
                else
                {
                    groupedByDate.Add(tbl.DATE, new tbl1());
                    groupedByDate.TryGetValue(tbl.DATE, out var groupedToBeAdded);
                    groupedToBeAdded.DATE = tbl.DATE;
                    groupedToBeAdded.Amount = groupedToBeAdded.Amount + tbl.Amount;
                    if (tbl.InterestPercent > groupedToBeAdded.InterestPercent) groupedToBeAdded.InterestPercent = tbl.InterestPercent;
                    if (tbl.DaysInYear > groupedToBeAdded.DaysInYear) groupedToBeAdded.DaysInYear = tbl.DaysInYear;
                    groupedToBeAdded.f_EOM += tbl.f_EOM;
                    if (groupedToBeAdded.f_EOM > 0) groupedToBeAdded.f_EOM = 1; else groupedToBeAdded.f_EOM = 0;

                    groupedToBeAdded.f_DueDate += tbl.f_DueDate;
                    if (groupedToBeAdded.f_DueDate > 0) groupedToBeAdded.f_DueDate = 1; else groupedToBeAdded.f_DueDate = 0;

                    groupedToBeAdded.f_EOY += tbl.f_EOY;
                    if (groupedToBeAdded.f_EOY > 0) groupedToBeAdded.f_EOY = 1; else groupedToBeAdded.f_EOY = 0;

                    groupedToBeAdded.f_CalcDate += tbl.f_CalcDate;
                    if (groupedToBeAdded.f_CalcDate > 0) groupedToBeAdded.f_CalcDate = 1; else groupedToBeAdded.f_CalcDate = 0;

                    groupedToBeAdded.f_Amount += tbl.f_Amount;
                    if (groupedToBeAdded.f_Amount > 0) groupedToBeAdded.f_Amount = 1; else groupedToBeAdded.f_Amount = 0;

                    groupedToBeAdded.f_Interest += tbl.f_Interest;
                    if (groupedToBeAdded.f_Interest > 0) groupedToBeAdded.f_Interest = 1; else groupedToBeAdded.f_Interest = 0;
                    groupedByDate[tbl.DATE] = groupedToBeAdded;
                }
            }


            foreach (var item in groupedByDate.Values)
            {
                tmpTbl2.Add(new tbl2 { DATE = item.DATE, Amount = item.Amount, InterestPercent = (decimal)(item.InterestPercent == null ? 0 : item.InterestPercent), DaysInYear = item.DaysInYear, f_EOM = item.f_EOM, f_DueDate = item.f_DueDate, f_EOY = item.f_EOY, f_CalcDate = item.f_CalcDate, f_Amount = item.f_Amount, f_Interest = item.f_Interest });    
            }

            tmpTbl2 = tmpTbl2.OrderBy(s => s.DATE).ToList();
            int TotalDaysInLeapYear = 0;
            decimal InterestBalanceInLeapYear = 0;
            DateTime pDate;
            decimal ? pInterestPercent;
            int pDaysInYear, Days = 0;
            decimal? SumInterestAmount = 0;
            decimal? AmountBalance = 0, pInterestBalance = 0;
            decimal? InterestBalance = 0;
            decimal? InterestAmount = 0;
            decimal? Amount = 0;
            int DaysInYear;
            decimal AddedInterest = 0, InterestDue = 0;
            bool f_DueDate, f_Amount, f_Interest, pf_DueDate = false, pf_Amount = false, pf_Interest = false;

            tmpTbl2.RemoveAll(s => s.DATE > DateTo);

            var top1 = tmpTbl2.OrderBy(s => s.DATE).FirstOrDefault();
            pDate = top1.DATE;
            AmountBalance = top1.Amount;
            pInterestPercent = top1.InterestPercent;
            pDaysInYear = top1.DaysInYear;
            f_DueDate = top1.f_DueDate > 0 ? true : false;
            f_Amount = top1.f_Amount > 0 ? true : false;

            Date = pDate;

            SumInterestAmount = (Math.Round((decimal)tmpTbl2.FindAll(s => s.DATE < Date).Sum(s => s.InterestAmount), 6));
            while (Date != null)
            {
                Date = DateTime.MinValue;
                DaysInYear = 0;
                InterestPercent = 0;

                var ssf = tmpTbl2.OrderBy(s => s.DATE).FirstOrDefault(s => s.DATE > pDate);

                if (ssf == null)
                    break;

                Date = ssf.DATE;   
                Amount = ssf.Amount;
                InterestPercent = ssf.InterestPercent;
                DaysInYear = ssf.DaysInYear;
                f_DueDate = ssf.f_DueDate > 0 ? true : false;
                f_Amount = ssf.f_Amount > 0 ? true : false;
                f_Interest = ssf.f_Interest > 0 ? true : false;

                SumInterestAmount += (Math.Round((tmpTbl2.First(s => s.DATE == pDate).InterestAmount), 6));

                Days = (int)(Date - pDate).TotalDays;
                InterestPercent = InterestPercent == 0 ? pInterestPercent : InterestPercent;
                DaysInYear = (CalculationMethod == 1 || CalculationMethod == 4) ? DateTime.ParseExact("31.12." + Date.Year.ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).DayOfYear : 360;

                TotalDaysInLeapYear += 1;

                if ((DateTime.ParseExact("31.12." + Date.Year.ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).DayOfYear !=
                        DateTime.ParseExact("31.12." + pDate.Year.ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture).DayOfYear &&
                        (CalculationMethod == 1 || CalculationMethod == 4)) ||
                        (pf_DueDate && InterestPaymentType == 2) ||
                        pf_Amount ||
                        pf_Interest ||
                        (CalculationMethod == 2 && pDate == pDate.AddMonths(1).AddDays(-1)))
                {
                    TotalDaysInLeapYear = 1;
                }

                if (TotalDaysInLeapYear == 1 && Date > DateFrom.AddDays(1))
                {
                    var q = tmpTbl2.First(s => s.DATE == pDate);
                    InterestBalanceInLeapYear = q == null ? 0 : q.InterestBalance;
                }

                if (CalculationMethod == 1)
                {
                    InterestBalance = (Math.Round((decimal)AmountBalance, 6) * Math.Round((decimal)pInterestPercent, 6) / 100 / DaysInYear * TotalDaysInLeapYear) + Math.Round((decimal)InterestBalanceInLeapYear, 6);
                }

                if (CalculationMethod == 2)
                {
                    InterestBalance = (Math.Round((decimal)AmountBalance, 6) * Math.Round((decimal)pInterestPercent, 6) / 100 / DaysInYear * 30 / DateTime.DaysInMonth(pDate.Year, pDate.Month) * TotalDaysInLeapYear) + Math.Round((decimal)InterestBalanceInLeapYear, 6);
                }

                if (CalculationMethod == 3)
                {
                    InterestBalance = (Math.Round((decimal)AmountBalance, 6) * Math.Round((decimal)pInterestPercent, 6) / 100 / DaysInYear * TotalDaysInLeapYear) + Math.Round((decimal)InterestBalanceInLeapYear, 6);
                }

                if (CalculationMethod == 4)
                {
                    InterestBalance = (Math.Round((decimal)AmountBalance, 6) * Math.Round((decimal)pInterestPercent, 6) / 100 / DaysInYear * TotalDaysInLeapYear) + Math.Round((decimal)InterestBalanceInLeapYear, 6);
                }

                int[] tmp = new int[] { 1, 2, 3, 4 };
                if (tmp.Contains((int)CalculationMethod))
                {
                    InterestAmount = Math.Round((decimal)InterestBalance, 6) - Math.Round((decimal)SumInterestAmount, 6);
                }
                else
                {
                    InterestBalance = Math.Round((decimal)InterestBalance, 6) + Math.Round((decimal)InterestAmount, 6);
                }

                if (f_DueDate)
                {
                    TotalDue = (decimal)(InterestBalance - tmpTbl2.Where(x => x.f_DueDate == 1).Sum(x => x.InterestDue));
                }

                if (InterestPaymentType == 2 && f_DueDate)
                {
                    InterestDue = Math.Round((decimal)InterestBalance, RoundNumber) - Math.Round(AddedInterest, RoundNumber);
                    AddedInterest = Math.Round(AddedInterest, RoundNumber) + Math.Round(InterestDue, RoundNumber);
                }

                int index = tmpTbl2.FindIndex(s => s.DATE == Date);

                if(index != -1)
                {
                    var replace = tmpTbl2.ElementAt(index);
                    replace.AmountBalance = Math.Round((decimal)AmountBalance, 6);
                    replace.Amount = Math.Round((decimal)Amount , 6);
                    replace.Days = Days;
                    replace.InterestPercent = Math.Round((decimal)InterestPercent, 6);
                    replace.DaysInYear = DaysInYear;
                    replace.InterestAmount = Math.Round((decimal)InterestAmount, 6);
                    replace.InterestBalance = Math.Round((decimal)InterestBalance, 6);
                    replace.InterestDue = (f_DueDate == true) ? Math.Round(TotalDue, 2) : 0;
                    replace.TotalDaysInLeapYears = TotalDaysInLeapYear;
                    replace.InterestBalanceInLeapYear = Math.Round(InterestBalanceInLeapYear, 2);
                    tmpTbl2[index] = replace;
                }

                if (InterestPaymentType == 2 && f_Amount == false)
                {
                    AmountBalance = Math.Round((decimal)((Math.Round((decimal)AmountBalance, 6)) + (Math.Round((decimal)Amount, 6)) + (Math.Round((decimal)InterestDue, 6))), 6);
                }
                else
                {
                    AmountBalance = Math.Round((decimal)((Math.Round((decimal)AmountBalance, 6)) + (Math.Round((decimal)Amount, 6))), 6);
                }

                pDate = Date;
                pf_DueDate = f_DueDate;
                pf_Amount = f_Amount;
                pf_Interest = f_Interest;
                pInterestPercent = InterestPercent;
                pDaysInYear = DaysInYear;
                InterestDue = 0;

            }

            return tmpTbl2.OrderBy(s => s.DATE).ToList();
        }
    }

    internal class tbl1
    {
        public DateTime DATE { get; set; }
        public decimal? Amount { get; set; } = 0;
        public decimal? InterestPercent { get; set; }
        public int DaysInYear { get; set; } = 0;
        public int f_EOM { get; set; } = 0;
        public int f_DueDate { get; set; } = 0;
        public int f_EOY { get; set; } = 0;
        public int f_CalcDate { get; set; } = 0;
        public int f_Amount { get; set; } = 0;
        public int f_Interest { get; set; } = 0;
    }

    internal class tbl2
    {
        public DateTime DATE { get; set; }
        public decimal? Amount { get; set; } = 0;
        public decimal InterestPercent { get; set; }
        public int DaysInYear { get; set; } = 0;
        public decimal AmountBalance { get; set; } = 0;
        public int Days { get; set; } = 0;
        public int TotalDaysInLeapYears { get; set; } = 0;
        public decimal InterestAmount { get; set; } = 0;
        public decimal InterestDue { get; set; } = 0;
        public decimal InterestBalance { get; set; } = 0;
        public decimal InterestBalanceInLeapYear { get; set; } = 0;
        public int f_EOM { get; set; } = 0;
        public int f_DueDate { get; set; } = 0;
        public int f_EOY { get; set; } = 0;
        public int f_CalcDate { get; set; } = 0;
        public int f_Amount { get; set; } = 0;
        public int f_Interest { get; set; } = 0;
    }
}
