using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Mox.UI
{
    public class ClippingBorder : Border
    {
        private RectangleGeometry m_clipRect = new RectangleGeometry();
        private object m_oldClip;

        public override UIElement Child
        {
            get
            {
                return base.Child;
            }
            set
            {
                if (Child != value)
                {
                    if (Child != null)
                    {
                        Child.SetValue(ClipProperty, m_oldClip);
                        m_oldClip = null;
                    }

                    base.Child = value;

                    if (Child != null)
                    {
                        m_oldClip = Child.ReadLocalValue(ClipProperty);
                    }                    
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            OnApplyChildClip();
            base.OnRender(dc);
        }

        protected virtual void OnApplyChildClip()
        {
            UIElement child = Child;
            if (child != null)
            {
                m_clipRect.RadiusX = m_clipRect.RadiusY = Math.Max(0.0, CornerRadius.TopLeft - (BorderThickness.Left * 0.5));
                m_clipRect.Rect = new Rect(Child.RenderSize);
                child.Clip = m_clipRect;
            }
        }
    }
}
