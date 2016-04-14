#pragma warning disable 1036
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

        public int CompareTo(IBranch other)
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(IBranch other)
        {
            throw new NotImplementedException();
        }
        
        public bool Equals([AllowNull]IBranch other)
        {
            return this == other;
        }
    }
}
