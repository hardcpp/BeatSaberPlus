using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CP_SDK.UI.DefaultComponents.Subs
{
    /// <summary>
    /// Vertical scroll view content updater
    /// </summary>
    public class SubVScrollViewContent : MonoBehaviour
    {
        /// <summary>
        /// Is layout dirty
        /// </summary>
        private bool m_IsDirty;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Associated vertical scroll view
        /// </summary>
        public Components.CVScrollView VScrollView;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Component first frame
        /// </summary>
        private void Start()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            StopAllCoroutines();
            StartCoroutine(Coroutine_SetupScrollView());
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On frame
        /// </summary>
        private void Update()
        {
            if (!m_IsDirty)
                return;

            m_IsDirty = false;
            UpdateScrollView();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// On layout changed
        /// </summary>
        private void OnRectTransformDimensionsChange() => m_IsDirty = true;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Setup late coroutine
        /// </summary>
        /// <returns></returns>
        private IEnumerator Coroutine_SetupScrollView()
        {
            var l_RTransform = transform.GetChild(0) as RectTransform;
            yield return new WaitWhile(() => l_RTransform.sizeDelta.y == -1f);

            UpdateScrollView();
        }
        /// <summary>
        /// Update scroll view content size & buttons
        /// </summary>
        private void UpdateScrollView()
        {
            VScrollView.SetContentSize((transform.GetChild(0) as RectTransform).rect.height);
            VScrollView.RefreshScrollButtons();
        }
    }
}