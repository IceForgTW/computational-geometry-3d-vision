function [x1_hat, x2_hat] = Find_Optimal_Correspondence(x1, x2, F)
% Find closest correspondence (x1_hat, x2_hat) to (x1, x2)
% in which x2_hat'*F*x1_hat = 0
% Input:
%   - (x1, x2): correspondence
%   - F: fundamental matrix
% Output:
%   - (x1_hat, x2_hat): new correspondence
T = [1 0 -x1(1); 0 1 -x1(2); 0 0 1];
T_prime = [1 0 -x2(1); 0 1 -x2(2); 0 0 1];
F = inv(T_prime)'*F/T;
e = null(F);
e_prime = null(F');
e = e./sqrt(e(1)^2 + e(2)^2);
e_prime = e_prime./sqrt(e_prime(1)^2 + e_prime(2)^2);

R = [e(1) e(2) 0; -e(2) e(1) 0; 0 0 1];
R_prime = [e_prime(1) e_prime(2) 0; -e_prime(2) e_prime(1) 0; 0 0 1];
F = R_prime*F*R';

f = e(3);
f_prime = e_prime(3);
a = F(2, 2);
b = F(2, 3);
c = F(3, 2);
d = F(3, 3);

coeff = [-a^2*c*d*f^4 + a*b*c^2*f^4,...
    -a^2*d^2*f^4+b^2*c^2*f^4+f_prime^4*c^4+2*f_prime^2*a^2*c^2+a^4,...
    -a*b*d^2*f^4+b^2*c*d*f^4-2*a^2*c*d*f^2+2*a*b*c^2*f^2+4*f_prime^4*c^3*d+4*f_prime^2*a^2*c*d+4*f_prime^2*a*b*c^2+4*a^3*b,...
    -2*a^2*d^2*f^2+2*b^2*c^2*f^2+6*f_prime^4*c^2*d^2+2*f_prime^2*a^2*d^2+8*f_prime^2*a*b*c*d+2*f_prime^2*b^2*c^2+6*a^2*b^2,...
    -2*a*b*d^2*f^2+2*b^2*c*d*f^2+4*f_prime^4*c*d^3+4*f_prime^2*a*b*d^2+4*f_prime^2*b^2*c*d-a^2*c*d+a*b*c^2+4*a*b^3,...
    f_prime^4*d^4+2*f_prime^2*b^2*d^2-a^2*d^2+b^2*c^2+b^4,...
    -a*b*d^2+b^2*c*d
    ];

t = real(roots(coeff));
s = zeros(1, length(t)+1);
for i=1:length(t)
    s(i) = t(i)^2/(1+f^2*t(i)^2) + (c*t(i) + d)^2/((a*t(i)+b)^2+f_prime^2*(c*t(i)+d)^2);
end
s(length(t)+1) = 1/(f^2)+c^2/(a^2+f_prime^2*c^2);

t_min = min(s);
if (t_min == s(length(t)+1))
    l = [f 0 -1]';
    l_prime = [-f_prime*c a c]';
else
    l = [t_min*f 1 -t_min]';
    l_prime = [-f_prime*(c*t_min+d) a*t_min+b c*t_min+d]';
end

x1_hat = [-l(1)*l(3) -l(2)*l(3) l(1)^2+l(2)^2]';
x2_hat = [-l_prime(1)*l_prime(3) -l_prime(2)*l_prime(3) l_prime(1)^2+l_prime(2)^2]';

x1_hat = [T\(R'*x1_hat)];
x2_hat = [T_prime\(R_prime'*x2_hat)];

x1_hat = HNormalize(x1_hat);
x2_hat = HNormalize(x2_hat);