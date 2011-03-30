﻿using System.Windows;
using System.Windows.Controls;

namespace Mox.UI
{
    public class Transition : DependencyObject
    {
        #region Dependency Properties

        public bool ClipToBounds
        {
            get { return (bool)GetValue(ClipToBoundsProperty); }
            set { SetValue(ClipToBoundsProperty, value); }
        }

        public static readonly DependencyProperty ClipToBoundsProperty = DependencyProperty.Register("ClipToBounds", typeof(bool), typeof(Transition), new UIPropertyMetadata(false));

        public bool IsNewContentTopmost
        {
            get { return (bool)GetValue(IsNewContentTopmostProperty); }
            set { SetValue(IsNewContentTopmostProperty, value); }
        }

        public static readonly DependencyProperty IsNewContentTopmostProperty = DependencyProperty.Register("IsNewContentTopmost", typeof(bool), typeof(Transition), new UIPropertyMetadata(true));

        #endregion

        #region Methods

        // Called when an element is Removed from the TransitionPresenter's visual tree
        protected internal virtual void BeginTransition(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent)
        {
            MidTransition(transitionElement, oldContent, newContent);
            EndTransition(transitionElement, oldContent, newContent);
        }

        //Transitions should call this method when the old content has disappeared but before the new one appears
        protected void MidTransition(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent)
        {
            transitionElement.OnMidTransitionCompleted();
        }

        //Transitions should call this method when they are done
        protected void EndTransition(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent)
        {
            OnTransitionEnded(transitionElement, oldContent, newContent);

            transitionElement.OnTransitionCompleted();
        }

        //Transitions can override this to perform cleanup at the end of the transition
        protected virtual void OnTransitionEnded(TransitionPresenter transitionElement, ContentPresenter oldContent, ContentPresenter newContent)
        {
        }

        #endregion
    }
}