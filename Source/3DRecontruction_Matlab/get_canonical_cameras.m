function [P,Pprime] = get_canonical_cameras( F )
  % Get the "canonical" cameras for given fundamental matrix
  % according to Hartley and Zisserman (2004), p256, Result 9.14
 
  % But ensure that the left 3x3 submatrix of Pprime is nonsingular
  % using Result 9.15, that the general form is
  % [ skewsym( e12 ) * F + e12 * v', k * e12 ] where v is an arbitrary
  % 3-vector and k is an arbitrary scalar
 
  P = [ 1 0 0 0
        0 1 0 0
        0 0 1 0 ];
 
  e12 = null( F' );
  M = get_skew_symmetric_mat( e12 ) * F + e12 * [1 1 1];
  Pprime = [ M, e12 ];
end