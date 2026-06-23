# Next.js frontend build and runtime

FROM node:22 AS builder
WORKDIR /app

ARG NEXT_PUBLIC_API_BASE_URL=http://localhost:8080

COPY frontend/package*.json ./
RUN npm ci

COPY frontend/ ./
RUN NEXT_PUBLIC_API_BASE_URL=$NEXT_PUBLIC_API_BASE_URL npm run build

FROM node:22 AS runtime
WORKDIR /app

COPY frontend/package*.json ./
RUN npm ci --only=production

COPY --from=builder /app/.next ./.next
COPY --from=builder /app/public ./public

ENV NODE_ENV=production
EXPOSE 3000

CMD ["npm", "run", "start"]
