﻿using HeBianGu.Base.WpfBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HeBianGu.General.WpfControlLib
{
    /// <summary>
    ///     页码
    /// </summary>
    [TemplatePart(Name = ElementButtonLeft, Type = typeof(Button))]
    [TemplatePart(Name = ElementButtonRight, Type = typeof(Button))]
    [TemplatePart(Name = ElementButtonFirst, Type = typeof(RadioButton))]
    [TemplatePart(Name = ElementTextBlockLeft, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementPanelMain, Type = typeof(Panel))]
    [TemplatePart(Name = ElementTextBlockRight, Type = typeof(TextBlock))]
    [TemplatePart(Name = ElementButtonLast, Type = typeof(RadioButton))]
    public class Pagination : Control
    {
        #region Constants

        private const string ElementButtonLeft = "PART_ButtonLeft";
        private const string ElementButtonRight = "PART_ButtonRight";
        private const string ElementButtonFirst = "PART_ButtonFirst";
        private const string ElementTextBlockLeft = "PART_TextBlockLeft";
        private const string ElementPanelMain = "PART_PanelMain";
        private const string ElementTextBlockRight = "PART_TextBlockRight";
        private const string ElementButtonLast = "PART_ButtonLast";

        #endregion Constants

        #region Data

        private Button _buttonLeft;
        private Button _buttonRight;
        private RadioButton _buttonFirst;
        private TextBlock _textBlockLeft;
        private Panel _panelMain;
        private TextBlock _textBlockRight;
        private RadioButton _buttonLast;

        private bool _appliedTemplate;

        #endregion Data

        #region Public Events

        /// <summary>
        ///     页面更新事件
        /// </summary>
        public static readonly RoutedEvent PageUpdatedEvent =
            EventManager.RegisterRoutedEvent("PageUpdated", RoutingStrategy.Bubble,
                typeof(EventHandler<FunctionEventArgs<int>>), typeof(Pagination));

        /// <summary>
        ///     页面更新事件
        /// </summary>
        public event EventHandler<FunctionEventArgs<int>> PageUpdated
        {
            add => AddHandler(PageUpdatedEvent, value);
            remove => RemoveHandler(PageUpdatedEvent, value);
        }

        #endregion Public Events

        public Pagination()
        {
            CommandBindings.Add(new CommandBinding(CommandService.Prev, ButtonPrev_OnClick));
            CommandBindings.Add(new CommandBinding(CommandService.Next, ButtonNext_OnClick));
            CommandBindings.Add(new CommandBinding(CommandService.Selected, ToggleButton_OnChecked));
            this.VisibilityWith(MaxPageCount > 1);
        }

        #region Public Properties

        #region MaxPageCount

        /// <summary>
        ///     最大页数
        /// </summary>
        public static readonly DependencyProperty MaxPageCountProperty = DependencyProperty.Register(
            "MaxPageCount", typeof(int), typeof(Pagination), new PropertyMetadata(1, (o, args) =>
            {
                if (o is Pagination && args.NewValue is int)
                {
                    Pagination pagination = o as Pagination;
                    int value = (int)args.NewValue;

                    if (pagination.PageIndex > pagination.MaxPageCount)
                    {
                        pagination.PageIndex = pagination.MaxPageCount;
                    }

                    pagination.VisibilityWith(value > 1);
                    pagination.Update();
                }
            }, (o, value) =>
            {
                if (!(o is Pagination)) return 1;
                var intValue = (int)value;
                if (intValue < 1)
                {
                    return 1;
                }
                return intValue;
            }));

        /// <summary>
        ///     最大页数
        /// </summary>
        public int MaxPageCount
        {
            get => (int)GetValue(MaxPageCountProperty);
            set => SetValue(MaxPageCountProperty, value);
        }

        #endregion MaxPageCount

        #region DataCountPerPage

        /// <summary>
        ///     每页的数据量
        /// </summary>
        public static readonly DependencyProperty DataCountPerPageProperty = DependencyProperty.Register(
            "DataCountPerPage", typeof(int), typeof(Pagination), new PropertyMetadata(20, (o, args) =>
            {
                if (o is Pagination)
                {
                    Pagination pagination = o as Pagination;
                    pagination.Update();
                }
            }, (o, value) =>
            {
                if (!(o is Pagination)) return 1;
                var intValue = (int)value;
                if (intValue < 1)
                {
                    return 1;
                }
                return intValue;
            }));

        /// <summary>
        ///     每页的数据量
        /// </summary>
        public int DataCountPerPage
        {
            get => (int)GetValue(DataCountPerPageProperty);
            set => SetValue(DataCountPerPageProperty, value);
        }

        #endregion

        #region PageIndex

        /// <summary>
        ///     当前页
        /// </summary>
        public static readonly DependencyProperty PageIndexProperty = DependencyProperty.Register(
            "PageIndex", typeof(int), typeof(Pagination), new PropertyMetadata(1, (o, args) =>
            {
                if (o is Pagination && args.NewValue is int)
                {
                    Pagination pagination = o as Pagination;

                    int value = (int)args.NewValue;

                    pagination.Update();
                    pagination.RaiseEvent(new FunctionEventArgs<int>(PageUpdatedEvent, pagination)
                    {
                        Info = value
                    });
                }
            }, (o, value) =>
            {
                if (!(o is Pagination)) return 1;

                var pagination = o as Pagination;

                var intValue = (int)value;
                if (intValue < 0)
                {
                    return 0;
                }
                if (intValue > pagination.MaxPageCount) return pagination.MaxPageCount;
                return intValue;
            }));

        /// <summary>
        ///     当前页
        /// </summary>
        public int PageIndex
        {
            get => (int)GetValue(PageIndexProperty);
            set => SetValue(PageIndexProperty, value);
        }

        #endregion PageIndex

        #region MaxPageInterval

        /// <summary>
        ///     表示当前选中的按钮距离左右两个方向按钮的最大间隔（4表示间隔4个按钮，如果超过则用省略号表示）
        /// </summary>       
        public static readonly DependencyProperty MaxPageIntervalProperty = DependencyProperty.Register(
            "MaxPageInterval", typeof(int), typeof(Pagination), new PropertyMetadata(3, (o, args) =>
            {
                if (o is Pagination)
                {
                    Pagination pagination = o as Pagination;

                    pagination.Update();
                }
            }), value =>
            {
                var intValue = (int)value;
                return intValue >= 0;
            });

        /// <summary>
        ///     表示当前选中的按钮距离左右两个方向按钮的最大间隔（4表示间隔4个按钮，如果超过则用省略号表示）
        /// </summary>   
        public int MaxPageInterval
        {
            get => (int)GetValue(MaxPageIntervalProperty);
            set => SetValue(MaxPageIntervalProperty, value);
        }

        #endregion MaxPageInterval

        #endregion

        #region Public Methods

        public override void OnApplyTemplate()
        {
            _appliedTemplate = false;
            base.OnApplyTemplate();

            _buttonLeft = GetTemplateChild(ElementButtonLeft) as Button;
            _buttonRight = GetTemplateChild(ElementButtonRight) as Button;
            _buttonFirst = GetTemplateChild(ElementButtonFirst) as RadioButton;
            _textBlockLeft = GetTemplateChild(ElementTextBlockLeft) as TextBlock;
            _panelMain = GetTemplateChild(ElementPanelMain) as Panel;
            _textBlockRight = GetTemplateChild(ElementTextBlockRight) as TextBlock;
            _buttonLast = GetTemplateChild(ElementButtonLast) as RadioButton;

            CheckNull();

            _appliedTemplate = true;
            Update();
        }

        #endregion Public Methods

        #region Private Methods

        private void CheckNull()
        {
            if (_buttonLeft == null || _buttonRight == null || _buttonFirst == null ||
                _textBlockLeft == null || _panelMain == null || _textBlockRight == null ||
                _buttonLast == null) throw new Exception();
        }

        /// <summary>
        ///     更新
        /// </summary>
        private void Update()
        {
            if (!_appliedTemplate) return;
            _buttonLeft.IsEnabled = PageIndex > 1;
            _buttonRight.IsEnabled = PageIndex < MaxPageCount;
            if (MaxPageInterval == 0)
            {
                _buttonFirst.Collapsed();
                _buttonLast.Collapsed();
                _textBlockLeft.Collapsed();
                _textBlockRight.Collapsed();
                _panelMain.Children.Clear();
                var selectButton = CreateButton(PageIndex);
                _panelMain.Children.Add(selectButton);
                selectButton.IsChecked = true;
                return;
            }
            _buttonFirst.Visibility = Visibility.Visible;
            _buttonLast.Visibility = Visibility.Visible;
            _textBlockLeft.Visibility = Visibility.Visible;
            _textBlockRight.Visibility = Visibility.Visible;

            //更新最后一页
            if (MaxPageCount == 1)
            {
                _buttonLast.Collapsed();
            }
            else
            {
                _buttonLast.Visible();
                _buttonLast.Tag = MaxPageCount.ToString();
            }

            //更新省略号
            var right = MaxPageCount - PageIndex;
            var left = PageIndex - 1;
            _textBlockRight.VisibilityWith(right > MaxPageInterval);
            _textBlockLeft.VisibilityWith(left > MaxPageInterval);

            //更新中间部分
            _panelMain.Children.Clear();
            if (PageIndex > 1 && PageIndex < MaxPageCount)
            {
                var selectButton = CreateButton(PageIndex);
                _panelMain.Children.Add(selectButton);
                selectButton.IsChecked = true;
            }
            else if (PageIndex == 1)
            {
                _buttonFirst.IsChecked = true;
            }
            else
            {
                _buttonLast.IsChecked = true;
            }

            var sub = PageIndex;
            for (var i = 0; i < MaxPageInterval - 1; i++)
            {
                if (--sub > 1)
                {
                    _panelMain.Children.Insert(0, CreateButton(sub));
                }
                else
                {
                    break;
                }
            }
            var add = PageIndex;
            for (var i = 0; i < MaxPageInterval - 1; i++)
            {
                if (++add < MaxPageCount)
                {
                    _panelMain.Children.Add(CreateButton(add));
                }
                else
                {
                    break;
                }
            }
        }

        private void ButtonPrev_OnClick(object sender, RoutedEventArgs e) => PageIndex--;

        private void ButtonNext_OnClick(object sender, RoutedEventArgs e) => PageIndex++;

        private RadioButton CreateButton(int page)
        {
            return new RadioButton
            {
                Style = this.TryFindResource("PaginationButtonStyle") as Style,
                Tag = page.ToString()
            };
        }

        private void ToggleButton_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!(e.OriginalSource is RadioButton button)) return;
            if (button.IsChecked == false) return;
            PageIndex = int.Parse(button.Tag.ToString());
        }

        #endregion Private Methods       
    }


    public class FunctionEventArgs<T> : RoutedEventArgs
    {
        public FunctionEventArgs(T info)
        {
            Info = info;
        }

        public FunctionEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
        {
        }

        public T Info { get; set; }
    }

    /// <summary> 翻页控件绑定项 </summary>
    public class DataPageViewModel : NotifyPropertyChanged
    {
        private int _minValue = 0;
        /// <summary> 当前页最小值  </summary>
        public int MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                RaisePropertyChanged("MinValue");
            }
        }

        private int _maxValue = 0;
        /// <summary> 当前页最大值  </summary>
        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                RaisePropertyChanged("MaxValue");
            }
        }


        private int _total = 0;
        /// <summary> 总数  </summary>
        public int Total
        {
            get { return _total; }
            set
            {
                _total = value;
                RaisePropertyChanged("Total");
            }
        }

        private int _totalPage = 0;
        /// <summary> 总页数  </summary>
        public int TotalPage
        {
            get { return _totalPage; }
            set
            {
                _totalPage = value;
                RaisePropertyChanged("TotalPage");
            }
        }


        private int _pageIndex = 0;
        /// <summary> 当前页索引  </summary>
        public int PageIndex
        {
            get { return _pageIndex; }
            set
            {
                _pageIndex = value;

                RaisePropertyChanged("PageIndex");

                this.Refresh();
            }
        }

        /// <summary> 每页的容量 </summary>
        public int Capacity { get; set; } = 10;

        /// <summary> 初始化 </summary>
        public void Init(int total)
        {
            this.Total = total;

            this.TotalPage = total / Capacity + (total % Capacity > 0 ? 1 : 0);

            this.PageIndex = 1;
        }

        /// <summary> 刷新页 </summary>
        void Refresh()
        {
            this.MinValue = this.PageIndex * this.Capacity - this.Capacity + 1;

            this.MaxValue = (this.MinValue + this.Capacity) > this.Total ? this.Total : this.MinValue + this.Capacity - 1;
        }

        /// <summary> 清理数据 </summary>
        public void Clear()
        {
            this.Total = 0;
            this.PageIndex = 0;
            this.MaxValue = 0;
            this.MinValue = 0;
            this.TotalPage = 0;
        }
    }
}
