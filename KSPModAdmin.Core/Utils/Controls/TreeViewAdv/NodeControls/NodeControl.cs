using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace KSPModAdmin.Core.Utils.Controls.Aga.Controls.Tree.NodeControls
{
	[DesignTimeVisible(false), ToolboxItem(false)]
	public abstract class NodeControl : Component
	{
		#region Properties

		private TreeViewAdv _parent;
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TreeViewAdv Parent
		{
			get { return _parent; }
			set 
			{
				if (value != _parent)
				{
					if (_parent != null)
						_parent.NodeControls.Remove(this);

					if (value != null)
						value.NodeControls.Add(this);
				}
			}
		}

		private IToolTipProvider _toolTipProvider;
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IToolTipProvider ToolTipProvider
		{
			get { return _toolTipProvider; }
			set { _toolTipProvider = value; }
		}

		private TreeColumn _parentColumn;
		public TreeColumn ParentColumn
		{
			get { return _parentColumn; }
			set 
			{ 
				_parentColumn = value; 
				if (_parent != null)
					_parent.FullUpdate();
			}
		}

		private VerticalAlignment _verticalAlign = VerticalAlignment.Center;
		[DefaultValue(VerticalAlignment.Center)]
		public VerticalAlignment VerticalAlign
		{
			get { return _verticalAlign; }
			set 
			{ 
				_verticalAlign = value;
				if (_parent != null)
					_parent.FullUpdate();
			}
        }

        private HorizontalAlignment _horizontalAlign = HorizontalAlignment.Left;
        [DefaultValue(HorizontalAlignment.Left)]
        public HorizontalAlignment HorizontalAlign
        {
            get { return _horizontalAlign; }
            set
            {
                _horizontalAlign = value;
                if (_parent != null)
                    _parent.FullUpdate();
            }
        }

		private int _leftMargin = 0;
		public int LeftMargin
		{
			get { return _leftMargin; }
			set 
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException();

				_leftMargin = value;
				if (_parent != null)
					_parent.FullUpdate();
			}
		}
		#endregion

		internal virtual void AssignParent(TreeViewAdv parent)
		{
			_parent = parent;
		}

		protected virtual Rectangle GetBounds(TreeNodeAdv node, DrawContext context)
		{
			Rectangle r = context.Bounds;
			Size s = GetActualSize(node, context);
			Size bs = new Size(r.Width - LeftMargin, Math.Min(r.Height, s.Height));

		    Rectangle rect = Rectangle.Empty;
			switch (VerticalAlign)
			{
				case VerticalAlignment.Top:
                    rect = new Rectangle(new Point(r.X + LeftMargin, r.Y), bs);
			        break;
				case VerticalAlignment.Bottom:
                    rect = new Rectangle(new Point(r.X + LeftMargin, r.Bottom - s.Height), bs);
                    break;
                default:
                    rect = new Rectangle(new Point(r.X + LeftMargin, r.Y + (r.Height - s.Height) / 2), bs);
                    break;
            }
		    switch (HorizontalAlign)
		    {
		        case HorizontalAlignment.Center:
            		int newWidth = bs.Width - s.Width;
		            if (newWidth > 1)
		            {
                        int deltaToCenter = newWidth / 2;
		                rect = new Rectangle(new Point(r.X + deltaToCenter, rect.Y), new Size(bs.Width, bs.Height));
		            }
		            break;
		        case HorizontalAlignment.Right:
                    int deltaToRight = bs.Width - s.Width;
		            rect = new Rectangle(new Point(r.X + deltaToRight, rect.Y), new Size(bs.Width, bs.Height));
		            break;
		        default:
		            rect = new Rectangle(new Point(r.X, rect.Y), new Size(bs.Width, bs.Height));
		            break;
		    }

		    return rect;
		}

		protected void CheckThread()
		{
			if (Parent != null && Control.CheckForIllegalCrossThreadCalls)
				if (Parent.InvokeRequired)
					throw new InvalidOperationException("Cross-thread calls are not allowed");
		}

		public bool IsVisible(TreeNodeAdv node)
		{
			NodeControlValueEventArgs args = new NodeControlValueEventArgs(node);
			args.Value = true;
			OnIsVisibleValueNeeded(args);
			return Convert.ToBoolean(args.Value);
		}

		internal Size GetActualSize(TreeNodeAdv node, DrawContext context)
		{
			if (IsVisible(node))
			{
				Size s = MeasureSize(node, context);
				return new Size(s.Width + LeftMargin, s.Height);
			}
			else
				return Size.Empty;
		}

		public abstract Size MeasureSize(TreeNodeAdv node, DrawContext context);

		public abstract void Draw(TreeNodeAdv node, DrawContext context);

		public virtual string GetToolTip(TreeNodeAdv node)
		{
			if (ToolTipProvider != null)
				return ToolTipProvider.GetToolTip(node, this);
			else
				return string.Empty;
		}

		public virtual void MouseDown(TreeNodeAdvMouseEventArgs args)
		{
		}

		public virtual void MouseUp(TreeNodeAdvMouseEventArgs args)
		{
		}

		public virtual void MouseDoubleClick(TreeNodeAdvMouseEventArgs args)
		{
		}

		public virtual void KeyDown(KeyEventArgs args)
		{
		}

		public virtual void KeyUp(KeyEventArgs args)
		{
		}

		public event EventHandler<NodeControlValueEventArgs> IsVisibleValueNeeded;
		protected virtual void OnIsVisibleValueNeeded(NodeControlValueEventArgs args)
		{
			if (IsVisibleValueNeeded != null)
				IsVisibleValueNeeded(this, args);
		}
	}
}
