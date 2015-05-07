/*============================================================================== 
 * Copyright (c) 2012-2013 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using System.Collections;

public class CloudRecognitionUIView : UIView {
    
    #region PUBLIC_PROPERTIES
    public CameraDevice.FocusMode FocusMode
    {
        get {
            return m_focusMode;
        }
        set {
            m_focusMode = value;
        }
    }
    #endregion PUBLIC_PROPERTIES
    
    #region PUBLIC_MEMBER_VARIABLES
    public event System.Action TappedToClose;
    public UIBox mBox;
    public UICheckButton mAboutLabel;
    public UICheckButton mExtendedTracking;
    public UILabel mBackgroundTextureLabel;
    public UICheckButton mCameraFlashSettings;
    public UICheckButton mAutoFocusSetting;
    public UILabel mCameraLabel;
    public UIRadioButton mCameraFacing;
    public UIButton mCloseButton;
    #endregion PUBLIC_MEMBER_VARIABLES
    
    #region PRIVATE_MEMBER_VARIABLES
    private CameraDevice.FocusMode m_focusMode;
    #endregion PRIVATE_MEMBER_VARIABLES
    
    #region PUBLIC_METHODS
    
    public void LoadView()
    {
        mBox = new UIBox(UIConstants.BoxRect, UIConstants.MainBackground);
        
        mBackgroundTextureLabel = new UILabel(UIConstants.RectLabelOne, UIConstants.CloudRecognition);
        
        string [] extendedTrackingStyle = {UIConstants.ExtendedTrackingStyleOff, UIConstants.ExtendedTrackingStyleOn};
        mExtendedTracking = new UICheckButton(UIConstants.RectOptionOne, false, extendedTrackingStyle);
        
        string[] aboutStyles = { UIConstants.AboutLableStyle, UIConstants.AboutLableStyle };
        mAboutLabel = new UICheckButton(UIConstants.RectLabelAbout, false, aboutStyles);
        
        string[] cameraFlashStyles = {UIConstants.CameraFlashStyleOff, UIConstants.CameraFlashStyleOn};
        mCameraFlashSettings = new UICheckButton(UIConstants.RectOptionThree, false, cameraFlashStyles);
        
        string[] autofocusStyles = {UIConstants.AutoFocusStyleOff, UIConstants.AutoFocusStyleOn};
        mAutoFocusSetting = new UICheckButton(UIConstants.RectOptionTwo, false, autofocusStyles);
        
        mCameraLabel = new UILabel(UIConstants.RectLabelTwo, UIConstants.CameraLabelStyle);
        
        string[,] cameraFacingStyles = new string[2,2] {{UIConstants.CameraFacingFrontStyleOff, UIConstants.CameraFacingFrontStyleOn},{ UIConstants.CameraFacingRearStyleOff, UIConstants.CameraFacingRearStyleOn}};
        UIRect[] cameraRect = { UIConstants.RectOptionFour, UIConstants.RectOptionFive };
        mCameraFacing = new UIRadioButton(cameraRect, 1, cameraFacingStyles);
        
        string[] closeButtonStyles = {UIConstants.closeButtonStyleOff, UIConstants.closeButtonStyleOn };
        mCloseButton = new UIButton(UIConstants.CloseButtonRect, closeButtonStyles);    
    }
    
    public void UnLoadView()
    {
        mBackgroundTextureLabel = null;
        mExtendedTracking = null;
        mCameraFlashSettings = null;
        mAutoFocusSetting = null;
        mAboutLabel = null;
        mCameraLabel = null;
        mCameraFacing = null;
    }
    
    public void UpdateUI(bool tf)
    {
        if(!tf)
        {
            return;
        }
        
        mBox.Draw();
        mAboutLabel.Draw();
        mExtendedTracking.Draw();
        mBackgroundTextureLabel.Draw();
        mCameraFlashSettings.Draw();
        mAutoFocusSetting.Draw();
        mCloseButton.Draw();
        mCameraLabel.Draw();
        mCameraFacing.Draw();
    }

    public void OnTappedToClose ()
    {
        if(this.TappedToClose != null)
        {
            this.TappedToClose();
        }
    }
    #endregion PUBLIC_METHODS
}

