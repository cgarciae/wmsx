/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/

using UnityEngine;
using System.Collections;

public class CloudRecoSceneManager : MonoBehaviour {
    
    #region PUBLIC_MEMBER_VARIABLES
    public UIEventHandler m_UIEventHandler;
    public static ViewType mActiveViewType;
    public enum ViewType {UIVIEW, ARCAMERAVIEW};
    #endregion PUBLIC_MEMBER_VARIABLES;
    
    #region PRIVATE_MEMBER_VARIABLES
    private CameraDevice.FocusMode mFocusMode;
    #endregion PRIVATE_MEMBER_VARIABLES
    
    void Start () 
    {
        InputController.BackButtonTapped += OnBackButtonTapped;
        InputController.SingleTapped += OnSingleTapped;
        InputController.DoubleTapped += OnDoubleTapped;
        m_UIEventHandler.CloseView += OnTappedOnCloseButton;
        m_UIEventHandler.GoToAboutPage += OnAboutPageTapped;
        m_UIEventHandler.Bind();
        //Enable continuous autofocus on start;
        SetFocusModeToContinuousAuto();
        mActiveViewType = ViewType.ARCAMERAVIEW;
        
    }
    
    void OnDestroy()
    {
        InputController.BackButtonTapped -= OnBackButtonTapped;
        InputController.SingleTapped -= OnSingleTapped;
        InputController.DoubleTapped -= OnDoubleTapped;
        m_UIEventHandler.CloseView -= OnTappedOnCloseButton;
        m_UIEventHandler.GoToAboutPage -= OnAboutPageTapped;
        m_UIEventHandler.UnBind();
    }
    
    void OnGUI()
    {
        m_UIEventHandler.UpdateView(false);
        switch(mActiveViewType)
        {
            case ViewType.UIVIEW:
                m_UIEventHandler.UpdateView(true);
                break;
            
            case ViewType.ARCAMERAVIEW:
                break;
        }
    }
    
    void Update () 
    {
        InputController.UpdateInput();
    }
    
    private void OnDoubleTapped()
    {
        if(mActiveViewType == ViewType.ARCAMERAVIEW)
        {
            mActiveViewType = ViewType.UIVIEW;
        }
    }
    
    private void OnTappedOnCloseButton()
    {
        mActiveViewType = ViewType.ARCAMERAVIEW;
    }
    
    private void OnBackButtonTapped()
    {
        Application.LoadLevel("Vuforia-1-About");
    }
    
    //Setting focus mode to triggerauto unsets the continuous autofocus mode. So, we invoke continuous autofocus right after.
    private void OnSingleTapped()
    {
        StartCoroutine(SetFocusModeToTriggerAuto());
    }
    
    private IEnumerator SetFocusModeToTriggerAuto()
    {
        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO)) {
              mFocusMode = CameraDevice.FocusMode.FOCUS_MODE_TRIGGERAUTO;
        }
        
        Debug.Log("Focus Mode Changed To " + mFocusMode);
        
        yield return new WaitForSeconds(1.0f);
        
        SetFocusModeToContinuousAuto();
        
    }
    
    private void SetFocusModeToContinuousAuto()
    {
        if (CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO)) {
            mFocusMode = CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO;
        }
        
        Debug.Log("Focus Mode Changed To " + mFocusMode);
    }
    
    private void OnAboutPageTapped()
    {
        Application.LoadLevel("Vuforia-1-About");
    }
    
}