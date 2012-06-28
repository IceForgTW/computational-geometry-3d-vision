function X_hat = HNormalize(X)
nrows = size(X, 1);
X_hat = X./repmat(X(end,:),nrows,1);
end