﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using Avalonia.Data;
using Avalonia.Media;

namespace Avalonia.Controls.Models.TreeDataGrid
{
    public class TextCell<T> : NotifyingBase, ITextCell, IDisposable, IEditableObject
    {
        private readonly ISubject<BindingValue<T>>? _binding;
        private readonly IDisposable? _subscription;
        [AllowNull] private T _value;
        [AllowNull] private T _cancelValue;
        private bool _isEditing;

        public TextCell(T value)
        {
            _value = value;
            IsReadOnly = true;
        }

        public TextCell(
            ISubject<BindingValue<T>> binding,
            bool isReadOnly,
            TextTrimming textTrimming)
        {
            _binding = binding;
            IsReadOnly = isReadOnly;
            TextTrimming = textTrimming;

            _subscription = binding.Subscribe(x =>
            {
                if (x.HasValue)
                    Value = x.Value;
            });
        }

        public bool CanEdit => !IsReadOnly;
        public bool IsReadOnly { get; }
        public TextTrimming TextTrimming { get; }

        public T Value
        {
            get => _value;
            set
            {
                if (RaiseAndSetIfChanged(ref _value, value) && !IsReadOnly && !_isEditing)
                    _binding!.OnNext(value);
            }
        }

        object? ICell.Value => Value;

        public void BeginEdit()
        {
            if (!_isEditing && !IsReadOnly)
            {
                _isEditing = true;
                _cancelValue = Value;
            }
        }

        public void CancelEdit()
        {
            if (_isEditing)
            {
                Value = _cancelValue;
                _isEditing = false;
                _cancelValue = default;
            }
        }

        public void EndEdit()
        {
            if (_isEditing && !EqualityComparer<T>.Default.Equals(_value, _cancelValue))
            {
                _isEditing = false;
                _cancelValue = default;
                _binding!.OnNext(_value);
            }
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
