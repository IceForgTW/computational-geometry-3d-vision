function Img_new = rectify_transform( Img, H )

  % Peter's image transform wants a specific max width/height for the
  % image, but we want the natural width/height, so we have to calculate it.

  rows = size( Img, 1 );
  cols = size( Img, 2 );
  xnew = transform_bbox( H, rows, cols );

  sze = max( xnew' )' - min( xnew' )' + [ 1 ; 1 ];

  Img_new = imTrans( Img, H, [], max( sze ));
end