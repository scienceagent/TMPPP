using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelBookingSystem.Visitor;
using HotelBookingSystem.Commands;
using HotelBookingSystem.Interfaces;
using HotelBookingSystem.Models;
using System.Collections.Generic;

namespace HotelBookingSystem.ViewModels
{
    public class VisitorController : BaseViewModel
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IRoomRepository _roomRepo;
        private readonly IUserRepository _userRepo;
        
        private string _analyticsOutput = "Click 'Run Analytics' to process data using the Visitor pattern.";

        public VisitorController(IBookingRepository bookingRepo, IRoomRepository roomRepo, IUserRepository userRepo)
        {
            _bookingRepo = bookingRepo;
            _roomRepo = roomRepo;
            _userRepo = userRepo;

            RunAnalyticsCommand = new RelayCommand(_ => ExecuteAnalytics());
        }

        public string AnalyticsOutput
        {
            get => _analyticsOutput;
            set => SetProperty(ref _analyticsOutput, value);
        }

        public ICommand RunAnalyticsCommand { get; }

        private void ExecuteAnalytics()
        {
            AnalyticsOutput = "Processing data structures via Double Dispatch...";

            // 1. Create the Visitor
            var statsVisitor = new StatisticsVisitor(_roomRepo);

            // 2. Fetch data (the object structure)
            var bookings = _bookingRepo.GetAllBookings();
            var users = _userRepo.GetAll();
            
            // 3. Traverse the structure - each element ACCEPTS the visitor
            // This is where Double Dispatch happens: element.Accept(visitor) -> visitor.Visit(this)
            foreach (var b in bookings) b.Accept(statsVisitor);
            foreach (var u in users) u.Accept(statsVisitor);

            // 4. Get results from visitor
            AnalyticsOutput = statsVisitor.GetSummary();
        }
    }
}
