% Intrinsic and Extrinsic Camera Parameters
%
% This script file can be directly excecuted under Matlab to recover the camera intrinsic and extrinsic parameters.
% IMPORTANT: This file contains neither the structure of the calibration objects nor the image coordinates of the calibration points.
%            All those complementary variables are saved in the complete matlab data file Calib_Results.mat.
% For more information regarding the calibration model visit http://www.vision.caltech.edu/bouguetj/calib_doc/


%-- Focal length:
fc = [ 831.611933166011681 ; 832.442942906428698 ];

%-- Principal point:
cc = [ 504.903243544820896 ; 337.614401365979461 ];

%-- Skew coefficient:
alpha_c = 0.000000000000000;

%-- Distortion coefficients:
kc = [ -0.190231498525744 ; 0.103629077214176 ; -0.002863566661988 ; 0.001434530753579 ; 0.000000000000000 ];

%-- Focal length uncertainty:
fc_error = [ 1.578373227167375 ; 1.454267975821698 ];

%-- Principal point uncertainty:
cc_error = [ 1.868351461130028 ; 1.628774035113717 ];

%-- Skew coefficient uncertainty:
alpha_c_error = 0.000000000000000;

%-- Distortion coefficients uncertainty:
kc_error = [ 0.006181265188938 ; 0.028223522633517 ; 0.000415013826254 ; 0.000469889711094 ; 0.000000000000000 ];

%-- Image size:
nx = 1024;
ny = 683;


%-- Various other variables (may be ignored if you do not use the Matlab Calibration Toolbox):
%-- Those variables are used to control which intrinsic parameters should be optimized

n_ima = 8;						% Number of calibration images
est_fc = [ 1 ; 1 ];					% Estimation indicator of the two focal variables
est_aspect_ratio = 1;				% Estimation indicator of the aspect ratio fc(2)/fc(1)
center_optim = 1;					% Estimation indicator of the principal point
est_alpha = 0;						% Estimation indicator of the skew coefficient
est_dist = [ 1 ; 1 ; 1 ; 1 ; 0 ];	% Estimation indicator of the distortion coefficients


%-- Extrinsic parameters:
%-- The rotation (omc_kk) and the translation (Tc_kk) vectors for every calibration image and their uncertainties

%-- Image #1:
omc_1 = [ -2.196792e+00 ; -2.177377e+00 ; 8.070949e-02 ];
Tc_1  = [ -6.268213e+02 ; -2.062376e+02 ; 1.650643e+03 ];
omc_error_1 = [ 1.891814e-03 ; 1.804917e-03 ; 3.959018e-03 ];
Tc_error_1  = [ 3.718490e+00 ; 3.309459e+00 ; 3.569506e+00 ];

%-- Image #2:
omc_2 = [ 2.191685e+00 ; 2.207630e+00 ; 4.302363e-02 ];
Tc_2  = [ -3.348629e+02 ; -2.222202e+02 ; 1.577418e+03 ];
omc_error_2 = [ 1.785847e-03 ; 1.607573e-03 ; 4.022062e-03 ];
Tc_error_2  = [ 3.574118e+00 ; 3.109047e+00 ; 3.034818e+00 ];

%-- Image #3:
omc_3 = [ -1.754034e+00 ; -1.910845e+00 ; 6.394767e-02 ];
Tc_3  = [ -4.679806e+02 ; -2.857663e+02 ; 1.864756e+03 ];
omc_error_3 = [ 1.561302e-03 ; 1.839659e-03 ; 2.949548e-03 ];
Tc_error_3  = [ 4.198321e+00 ; 3.689545e+00 ; 3.477695e+00 ];

%-- Image #4:
omc_4 = [ 1.709327e+00 ; 2.267838e+00 ; 6.081366e-01 ];
Tc_4  = [ -2.499743e+02 ; -3.153411e+02 ; 1.427858e+03 ];
omc_error_4 = [ 1.741571e-03 ; 1.621539e-03 ; 3.276888e-03 ];
Tc_error_4  = [ 3.234262e+00 ; 2.826191e+00 ; 2.812246e+00 ];

%-- Image #5:
omc_5 = [ -1.340988e+00 ; -1.695183e+00 ; 3.213647e-01 ];
Tc_5  = [ -4.886035e+01 ; -4.235821e+02 ; 1.737338e+03 ];
omc_error_5 = [ 1.487784e-03 ; 1.915916e-03 ; 2.284065e-03 ];
Tc_error_5  = [ 3.911985e+00 ; 3.391290e+00 ; 2.729573e+00 ];

%-- Image #6:
omc_6 = [ 1.863457e+00 ; 2.193438e+00 ; 4.426196e-01 ];
Tc_6  = [ -1.336046e+02 ; -2.557210e+02 ; 1.446156e+03 ];
omc_error_6 = [ 1.809059e-03 ; 1.594112e-03 ; 3.559054e-03 ];
Tc_error_6  = [ 3.260096e+00 ; 2.830121e+00 ; 2.680742e+00 ];

%-- Image #7:
omc_7 = [ 1.620125e+00 ; 2.331719e+00 ; 3.163779e-01 ];
Tc_7  = [ -3.798835e+02 ; -5.506391e+02 ; 1.756942e+03 ];
omc_error_7 = [ 1.378314e-03 ; 2.052088e-03 ; 3.365186e-03 ];
Tc_error_7  = [ 4.051217e+00 ; 3.494681e+00 ; 3.488398e+00 ];

%-- Image #8:
omc_8 = [ -2.094200e+00 ; -2.095649e+00 ; 4.778638e-01 ];
Tc_8  = [ -5.310722e+02 ; -3.320805e+02 ; 2.588865e+03 ];
omc_error_8 = [ 2.228331e-03 ; 1.755636e-03 ; 3.939156e-03 ];
Tc_error_8  = [ 5.835392e+00 ; 5.099461e+00 ; 4.587755e+00 ];

