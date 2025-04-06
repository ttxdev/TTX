docker build -t ghcr.io/ttxdev/ttx-frontend:main .
docker push ghcr.io/ttxdev/ttx-frontend:main
kubectl -n ttx rollout restart deployment/ttx-frontend
