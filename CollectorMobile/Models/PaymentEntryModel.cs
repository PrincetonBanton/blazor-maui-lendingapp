using System;
using System.ComponentModel;

namespace LendingApp.CollectorMobile.Models
{
    public class PaymentEntryModel : INotifyPropertyChanged
    {
        public Guid LoanId { get; set; }
        public Guid CollectorId { get; set; }

        private DateTime _paymentDate = DateTime.Now;
        public DateTime PaymentDate
        {
            get => _paymentDate;
            set
            {
                _paymentDate = value;
                OnPropertyChanged(nameof(PaymentDate));
            }
        }

        private double _paymentAmount;
        public double PaymentAmount
        {
            get => _paymentAmount;
            set
            {
                _paymentAmount = value;
                OnPropertyChanged(nameof(PaymentAmount));
            }
        }

        private string _remarks = string.Empty;
        public string Remarks
        {
            get => _remarks;
            set
            {
                _remarks = value;
                OnPropertyChanged(nameof(Remarks));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
