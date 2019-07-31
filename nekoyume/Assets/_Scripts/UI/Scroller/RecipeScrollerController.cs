﻿using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using Nekoyume.Data.Table;
using Nekoyume.UI.Model;

namespace Nekoyume.UI.Scroller
{
    public class RecipeScrollerController : MonoBehaviour, IEnhancedScrollerDelegate
    {
        public EnhancedScroller scroller;
        public RecipeCellView cellViewPrefab;

        private List<RecipeInfo> _recipeList = new List<RecipeInfo>();
        private float _cellViewHeight = 90f;

        #region Mono

        private void Awake()
        {
            this.ComponentFieldsNotNullTest();

            scroller.Delegate = this;
            _cellViewHeight = cellViewPrefab.GetComponent<RectTransform>().rect.height;
        }

        #endregion

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(cellViewPrefab) as RecipeCellView;
            if(ReferenceEquals(cellView, null))
            {
                throw new FailedToInstantiateGameObjectException(cellViewPrefab.name);
            }

            cellView.name = $"Cell {dataIndex}";
            cellView.SetData(_recipeList[dataIndex]);
            return cellView;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _cellViewHeight;
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return _recipeList.Count;
        }

        public void SetData(List<RecipeInfo> recipeList)
        {
            _recipeList = recipeList;
            scroller.ReloadData();
        }
    }
}
