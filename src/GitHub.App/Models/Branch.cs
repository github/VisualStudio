using System;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using NullGuard;
using ReactiveUI;

namespace GitHub.Models
{
    public class Branch : ReactiveObject, IBranch
    {
        public Branch() { }
        public Branch(string name) {
            Name = name;
        }
        public string Name { get; set; }
    }
}
