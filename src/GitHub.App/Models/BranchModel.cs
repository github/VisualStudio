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
    public class BranchModel : ReactiveObject, IBranchModel
    {
        public BranchModel() { }
        public BranchModel(string name) {
            Name = name;
        }
        public string Name { get; set; }

        public int CompareTo(IBranchModel other)
        {
            throw new NotImplementedException();
        }

        public void CopyFrom(IBranchModel other)
        {
            throw new NotImplementedException();
        }
        
        public bool Equals([AllowNull]IBranchModel other)
        {
            return this == other;
        }
    }
}
