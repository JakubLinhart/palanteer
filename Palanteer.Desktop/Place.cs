﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Palanteer.Desktop
{
    public class Place : INotifyPropertyChanged
    {
        private string name;
        private int x;
        private int y;

        public Place(bool canEdit = true)
        {
            CanEdit = canEdit;
        }

        public int X
        {
            get { return x; }
            set
            {
                x = value;
                OnPropertyChanged();
            }
        }

        public int Y
        {
            get { return y; }
            set
            {
                y = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        public bool CanEdit { get; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Select()
        {
            OnSelected();
        }

        public event EventHandler Selected;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }
    }
}