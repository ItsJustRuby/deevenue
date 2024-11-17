FROM postgres:17.1-alpine3.20

# Install nicer Postgres CLI tools just in case
RUN apk add --no-cache \
    pgcli=4.1.0-r0 \
    pspg=5.8.6-r0

# use pspg in `psql`
ENV PSQL_PAGER='pspg -X -b'
# use pspg in `pgcli`
ENV PAGER='pspg -X -b'
