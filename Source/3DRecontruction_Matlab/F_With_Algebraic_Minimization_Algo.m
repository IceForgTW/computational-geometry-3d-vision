function F = F_With_Algebraic_Minimization_Algo(x1, x2)
global fi epsiloni e_last;
%initialize: F0 and E0
F0 = F_From_normalized_8points(x1, x2);

A = [ repmat(x2(1,:)',1,3) .* x1', repmat(x2(2,:)',1,3) .* x1', x1(1:3,:)'];
[U,S,V] = svd(F0);
e0 = V(:,end);
e_last = e0;
fi = reshape(F0', 9, 1);
epsiloni = norm(A*fi);

e = lsqnonlin(@(e)costFunc(A, e),e0,[],[],optimset('Display','off','TolX',1e-4,'TolFun',1e-4,'MaxFunEval',1e4,'MaxIter',1e3,'Algorithm',{'levenberg-marquardt' 0.01}));

F = reshape(fi,3,3)';
end

%% function x = const_min_subject_span_space(A,G,r)
% finds the vector x that maximizes ||A x|| subject to the constraint ||x||=1 and x=G x_hat
% where G has rank r
function x = const_min_subject_span_space(A,G,r)
[u_g,s_g,v_g] = svd(G);
u_prime = u_g(:,1:r);

[u_a,s_a,v_a] = svd(A*u_prime);
x_p = v_a(:,end);

x = u_prime * x_p;
end

%% cost function
function err = costFunc(A, e)
global fi epsiloni e_last;
e_cross = get_skew_symmetric_mat(e);
E = [e_cross zeros(3) zeros(3); 
    zeros(3) e_cross zeros(3); 
    zeros(3) zeros(3) e_cross];
fi_t = const_min_subject_span_space(A, E, 6);

fi_t = fi_t * double(sign(e'*e_last));
err = norm(A*fi_t);

if (err < epsiloni)
    epsiloni = err;
    fi = fi_t;
    e_last = e;
end
end