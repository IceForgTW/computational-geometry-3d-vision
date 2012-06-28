function theta = decompose_rotation(R) 
theta = zeros(3, 1);
theta(1) = atan2(R(3,2), R(3,3)); 
theta(2) = atan2(-R(3,1), sqrt(R(3,2)*R(3,2) + R(3,3)*R(3,3))); 
theta(3) = atan2(R(2,1), R(1,1));
end