--
-- PostgreSQL database dump
--

-- Dumped from database version 13.1
-- Dumped by pg_dump version 13.1

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Name: sp_add_administrator(text, text, integer, bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_administrator(_first_name text, _last_name text, _level integer, _user_id bigint) RETURNS integer
    LANGUAGE plpgsql
    AS $$
            DECLARE
                record_id integer;

            BEGIN
                INSERT INTO administrators(first_name, last_name, level, user_id) values (_first_name, _last_name, _level, _user_id)
                    returning id into record_id;

                return record_id;
            END;
    $$;


ALTER FUNCTION public.sp_add_administrator(_first_name text, _last_name text, _level integer, _user_id bigint) OWNER TO postgres;

--
-- Name: sp_add_airline_company(text, integer, bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_airline_company(_name text, _country_id integer, _user_id bigint) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id bigint;
            BEGIN
                INSERT INTO airline_companies(name, country_id, user_id) values (_name, _country_id, _user_id)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_airline_company(_name text, _country_id integer, _user_id bigint) OWNER TO postgres;

--
-- Name: sp_add_country(text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_country(_name text) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id integer;

            BEGIN
                INSERT INTO countries(name) values (_name)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_country(_name text) OWNER TO postgres;

--
-- Name: sp_add_customer(text, text, text, text, text, bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_customer(_first_name text, _last_name text, _address text, _phone_number text, _credit_card_number text, _user_id bigint) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id bigint;
            BEGIN
                INSERT INTO customers(first_name, last_name, address, phone_number, credit_card_number, user_id) values (_first_name, _last_name, _address, _phone_number, _credit_card_number, _user_id)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_customer(_first_name text, _last_name text, _address text, _phone_number text, _credit_card_number text, _user_id bigint) OWNER TO postgres;

--
-- Name: sp_add_flight(bigint, integer, integer, timestamp without time zone, timestamp without time zone, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_flight(_airline_company_id bigint, _origin_country_id integer, _destination_country_id integer, _departure_time timestamp without time zone, _landing_time timestamp without time zone, _remaining_tickets integer) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id bigint;
            BEGIN
                INSERT INTO flights(airline_company_id, origin_country_id, destination_country_id, departure_time, landing_time, remaining_tickets) values (_airline_company_id, _origin_country_id, _destination_country_id, _departure_time, _landing_time, _remaining_tickets)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_flight(_airline_company_id bigint, _origin_country_id integer, _destination_country_id integer, _departure_time timestamp without time zone, _landing_time timestamp without time zone, _remaining_tickets integer) OWNER TO postgres;

--
-- Name: sp_add_flight_history(bigint, bigint, text, integer, integer, timestamp without time zone, timestamp without time zone, integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_flight_history(_original_id bigint, _airline_company_id bigint, _airline_company_name text, _origin_country_id integer, _destination_country_id integer, _departure_time timestamp without time zone, _landing_time timestamp without time zone, _remaining_tickets integer, _flight_status integer) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id bigint;
            BEGIN
                INSERT INTO flights_history(flight_original_id, airline_company_id, airline_company_name, origin_country_id, destination_country_id, departure_time, landing_time, remaining_tickets, flight_status) 
				values (_original_id,_airline_company_id, _airline_company_name, _origin_country_id, _destination_country_id, _departure_time, _landing_time, _remaining_tickets,_flight_status)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_flight_history(_original_id bigint, _airline_company_id bigint, _airline_company_name text, _origin_country_id integer, _destination_country_id integer, _departure_time timestamp without time zone, _landing_time timestamp without time zone, _remaining_tickets integer, _flight_status integer) OWNER TO postgres;

--
-- Name: sp_add_ticket(bigint, bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_ticket(_flight_id bigint, _customer_id bigint) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id bigint;
            BEGIN
                INSERT INTO tickets(flight_id, customer_id) values (_flight_id, _customer_id)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_ticket(_flight_id bigint, _customer_id bigint) OWNER TO postgres;

--
-- Name: sp_add_ticket_history(bigint, bigint, bigint, text, text, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_ticket_history(_original_id bigint, _flight_id bigint, _customer_id bigint, _customer_full_name text, _customer_username text, _ticket_status integer) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id bigint;
            BEGIN
                INSERT INTO tickets_history(original_ticket_id, flight_id, customer_id, customer_full_name, customer_username, ticket_status) values (_original_id, _flight_id, _customer_id, _customer_full_name, _customer_username, _ticket_status)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_ticket_history(_original_id bigint, _flight_id bigint, _customer_id bigint, _customer_full_name text, _customer_username text, _ticket_status integer) OWNER TO postgres;

--
-- Name: sp_add_user(text, text, text, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_add_user(_username text, _password text, _email text, _user_role_id integer) RETURNS bigint
    LANGUAGE plpgsql
    AS $$
DECLARE
                record_id bigint;

            BEGIN
                INSERT INTO users(username, password, email, user_role) values (_username, _password, _email, _user_role_id)
                    returning id into record_id;

                return record_id;
            END;
$$;


ALTER FUNCTION public.sp_add_user(_username text, _password text, _email text, _user_role_id integer) OWNER TO postgres;

--
-- Name: sp_clear_db(); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_clear_db()
    LANGUAGE plpgsql
    AS $$
BEGIN
TRUNCATE TABLE tickets RESTART IDENTITY CASCADE;
TRUNCATE TABLE flights RESTART IDENTITY CASCADE;
TRUNCATE TABLE airline_companies RESTART IDENTITY CASCADE;
TRUNCATE TABLE customers RESTART IDENTITY CASCADE;
TRUNCATE TABLE administrators RESTART IDENTITY CASCADE;
TRUNCATE TABLE users RESTART IDENTITY CASCADE;
TRUNCATE TABLE countries RESTART IDENTITY CASCADE;
TRUNCATE TABLE tickets_history RESTART IDENTITY CASCADE;
TRUNCATE TABLE flights_history RESTART IDENTITY CASCADE;

END;
$$;


ALTER PROCEDURE public.sp_clear_db() OWNER TO postgres;

--
-- Name: sp_get_administrator(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_administrator(_id integer) RETURNS TABLE(admin_id integer, first_name text, last_name text, level integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select a.id,a.first_name,a.last_name,a.level, u.id, u.username, u.password, u.email, u.user_role  from administrators a
                    join users u on u.id = a.user_id
                    where a.id =_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_administrator(_id integer) OWNER TO postgres;

--
-- Name: sp_get_administrator_by_user_id(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_administrator_by_user_id(_user_id bigint) RETURNS TABLE(admin_id integer, first_name text, last_name text, level integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id,a.first_name,a.last_name,a.level, u.id, u.username, u.password, u.email, u.user_role  from administrators a
                    join users u on u.id = a.user_id
                    where u.id =_user_id;
            END;
$$;


ALTER FUNCTION public.sp_get_administrator_by_user_id(_user_id bigint) OWNER TO postgres;

--
-- Name: sp_get_airline_company(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_airline_company(_id bigint) RETURNS TABLE(airline_company_id bigint, airline_company_name text, airline_company_country_id integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id, a.name, a.country_id, u.id, u.username, u.password, u.email, u.user_role  from airline_companies a
                    join users u on u.id = a.user_id
                    where a.id =_id;
            END;
$$;


ALTER FUNCTION public.sp_get_airline_company(_id bigint) OWNER TO postgres;

--
-- Name: sp_get_airline_company_by_name(text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_airline_company_by_name(_name text) RETURNS TABLE(airline_company_id bigint, airline_company_name text, airline_company_country_id integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id, a.name, a.country_id, u.id, u.username, u.password, u.email, u.user_role  from airline_companies a
                    join users u on u.id = a.user_id
                    where a.name =_name;
            END;
$$;


ALTER FUNCTION public.sp_get_airline_company_by_name(_name text) OWNER TO postgres;

--
-- Name: sp_get_airline_company_by_user_id(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_airline_company_by_user_id(_user_id bigint) RETURNS TABLE(airline_company_id bigint, airline_company_name text, airline_company_country_id integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id, a.name, a.country_id, u.id, u.username, u.password, u.email, u.user_role  from airline_companies a
                    join users u on u.id = a.user_id
                    where u.id =_user_id;
            END;
$$;


ALTER FUNCTION public.sp_get_airline_company_by_user_id(_user_id bigint) OWNER TO postgres;

--
-- Name: sp_get_airline_company_by_username(text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_airline_company_by_username(_username text) RETURNS TABLE(airline_company_id bigint, airline_company_name text, airline_company_country_id integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id, a.name, a.country_id, u.id, u.username, u.password, u.email, u.user_role  from airline_companies a
                    join users u on u.id = a.user_id
                    where u.username =_username;
            END;
$$;


ALTER FUNCTION public.sp_get_airline_company_by_username(_username text) OWNER TO postgres;

--
-- Name: sp_get_all_administrators(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_administrators() RETURNS TABLE(admin_id integer, first_name text, last_name text, level integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select a.id,a.first_name,a.last_name,a.level, u.id, u.username, u.password, u.email, u.user_role  from administrators a
                    join users u on u.id = a.user_id;
                END;
    $$;


ALTER FUNCTION public.sp_get_all_administrators() OWNER TO postgres;

--
-- Name: sp_get_all_airline_companies(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_airline_companies() RETURNS TABLE(airline_company_id bigint, airline_company_name text, airline_company_country_id integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id, a.name, a.country_id, u.id, u.username, u.password, u.email, u.user_role  from airline_companies a
                    join users u on u.id = a.user_id;
                END;
$$;


ALTER FUNCTION public.sp_get_all_airline_companies() OWNER TO postgres;

--
-- Name: sp_get_all_airline_companies_by_country(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_airline_companies_by_country(_country_id integer) RETURNS TABLE(airline_company_id bigint, airline_company_name text, airline_company_country_id integer, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id, a.name, a.country_id, u.id, u.username, u.password, u.email, u.user_role  from airline_companies a
                    join users u on u.id = a.user_id
                    where a.country_id=_country_id;
                END;
$$;


ALTER FUNCTION public.sp_get_all_airline_companies_by_country(_country_id integer) OWNER TO postgres;

--
-- Name: sp_get_all_countries(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_countries() RETURNS TABLE(id integer, name text)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select * from countries;
            END;
    $$;


ALTER FUNCTION public.sp_get_all_countries() OWNER TO postgres;

--
-- Name: sp_get_all_customers(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_customers() RETURNS TABLE(customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select a.id, a.first_name, a.last_name, a.address, a.phone_number, a.credit_card_number, u.id, u.username, u.password, u.email, u.user_role  from customers a
                    join users u on u.id = a.user_id;
                END;
    $$;


ALTER FUNCTION public.sp_get_all_customers() OWNER TO postgres;

--
-- Name: sp_get_all_flights(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_flights() RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id;
                END;
    $$;


ALTER FUNCTION public.sp_get_all_flights() OWNER TO postgres;

--
-- Name: sp_get_all_tickets(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_tickets() RETURNS TABLE(ticket_id bigint, flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer, customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                      select t.id, f.id, a.id, a.name, a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets,
                           c.id, c.first_name, c.last_name, c.address, c.phone_number, c.credit_card_number from tickets t
                    join flights f on f.id = t.flight_id
                    join airline_companies a on a.id = f.airline_company_id
                    join customers c on c.id = t.customer_id;
                END;
    $$;


ALTER FUNCTION public.sp_get_all_tickets() OWNER TO postgres;

--
-- Name: sp_get_all_users(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_all_users() RETURNS TABLE(id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select * from users;
            END;
    $$;


ALTER FUNCTION public.sp_get_all_users() OWNER TO postgres;

--
-- Name: sp_get_country(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_country(_id integer) RETURNS TABLE(id integer, name text)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select * from countries
                    where countries.id =_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_country(_id integer) OWNER TO postgres;

--
-- Name: sp_get_customer(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_customer(_id bigint) RETURNS TABLE(customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select a.id, a.first_name, a.last_name, a.address, a.phone_number, a.credit_card_number, u.id, u.username, u.password, u.email, u.user_role  from customers a
                    join users u on u.id = a.user_id
                    where a.id =_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_customer(_id bigint) OWNER TO postgres;

--
-- Name: sp_get_customer_by_phone_number(text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_customer_by_phone_number(_phone_number text) RETURNS TABLE(customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select c.id, c.first_name, c.last_name, c.address, c.phone_number, c.credit_card_number, u.id, u.username, u.password, u.email, u.user_role  from customers c
                    join users u on u.id = c.user_id
                    where c.phone_number =_phone_number;
            END;
$$;


ALTER FUNCTION public.sp_get_customer_by_phone_number(_phone_number text) OWNER TO postgres;

--
-- Name: sp_get_customer_by_user_id(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_customer_by_user_id(_user_id bigint) RETURNS TABLE(customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select a.id, a.first_name, a.last_name, a.address, a.phone_number, a.credit_card_number, u.id, u.username, u.password, u.email, u.user_role  from customers a
                    join users u on u.id = a.user_id
                    where u.id =_user_id;
            END;
$$;


ALTER FUNCTION public.sp_get_customer_by_user_id(_user_id bigint) OWNER TO postgres;

--
-- Name: sp_get_customer_by_username(text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_customer_by_username(_username text) RETURNS TABLE(customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select c.id, c.first_name, c.last_name, c.address, c.phone_number, c.credit_card_number, u.id, u.username, u.password, u.email, u.user_role  from customers c
                    join users u on u.id = c.user_id
                    where u.username =_username;
            END;
$$;


ALTER FUNCTION public.sp_get_customer_by_username(_username text) OWNER TO postgres;

--
-- Name: sp_get_customer_by_username_and_password(text, text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_customer_by_username_and_password(_username text, _password text) RETURNS TABLE(customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select a.id, a.first_name, a.last_name, a.address, a.phone_number, a.credit_card_number, u.id, u.username, u.password, u.email, u.user_role  from customers a
                    join users u on u.id = a.user_id
                    where u.username =_username and u.password=_password;
            END;
    $$;


ALTER FUNCTION public.sp_get_customer_by_username_and_password(_username text, _password text) OWNER TO postgres;

--
-- Name: sp_get_flight(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flight(_id bigint) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
                    where f.id =_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_flight(_id bigint) OWNER TO postgres;

--
-- Name: sp_get_flight_history_by_original_id(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flight_history_by_original_id(_original_id bigint) RETURNS TABLE(id bigint, flight_original_id bigint, airline_company_id bigint, airline_company_name text, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer, flight_status integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
            RETURN QUERY
            	select * from flights_history f
                where f.flight_original_id =_original_id;
            END;
$$;


ALTER FUNCTION public.sp_get_flight_history_by_original_id(_original_id bigint) OWNER TO postgres;

--
-- Name: sp_get_flights_by_airline_company(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flights_by_airline_company(_airline_company_id bigint) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
                    where a.id =_airline_company_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_flights_by_airline_company(_airline_company_id bigint) OWNER TO postgres;

--
-- Name: sp_get_flights_by_customer(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flights_by_customer(_customer_id bigint) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
                    join tickets t on t.flight_id=f.id
                    where t.customer_id =_customer_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_flights_by_customer(_customer_id bigint) OWNER TO postgres;

--
-- Name: sp_get_flights_by_departure_date(date); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flights_by_departure_date(_departure_date date) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
                    where f.departure_time:: date =_departure_date;
            END;
    $$;


ALTER FUNCTION public.sp_get_flights_by_departure_date(_departure_date date) OWNER TO postgres;

--
-- Name: sp_get_flights_by_destination_country(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flights_by_destination_country(_destination_country_id integer) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
                    where f.destination_country_id =_destination_country_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_flights_by_destination_country(_destination_country_id integer) OWNER TO postgres;

--
-- Name: sp_get_flights_by_landing_date(date); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flights_by_landing_date(_landing_date date) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
                    where f.landing_time:: date =_landing_date;
            END;
    $$;


ALTER FUNCTION public.sp_get_flights_by_landing_date(_landing_date date) OWNER TO postgres;

--
-- Name: sp_get_flights_by_origin_country(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flights_by_origin_country(_origin_country_id integer) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
                    where f.origin_country_id =_origin_country_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_flights_by_origin_country(_origin_country_id integer) OWNER TO postgres;

--
-- Name: sp_get_flights_with_tickets_that_landed(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_flights_with_tickets_that_landed(_seconds bigint) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer, ticket_id bigint, customer_id bigint, first_name text, last_name text, username text)
    LANGUAGE plpgsql
    AS $$
BEGIN
return query
	(SELECT f.id,
	 		f.airline_company_id,
	 		a.name,
	 		f.origin_country_id,
	 		f.destination_country_id,
	 		f.departure_time,
	 		f.landing_time,
	 		f.remaining_tickets,
	 		t.id,
	 		t.customer_id,
	 		c.first_name,
	 		c.last_name,
	 		u.username
	 FROM flights f
	 JOIN airline_companies a on f.airline_company_id=a.id
	 left JOIN tickets t on f.id = t.flight_id
	 JOIN customers c on t.customer_id=c.id
	 JOIN users u on c.user_id=u.id
	WHERE (SELECT EXTRACT(EPOCH FROM current_timestamp::timestamp without time zone) - EXTRACT(EPOCH FROM f.landing_time))>_seconds);
END;
$$;


ALTER FUNCTION public.sp_get_flights_with_tickets_that_landed(_seconds bigint) OWNER TO postgres;

--
-- Name: sp_get_ticket(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_ticket(_id bigint) RETURNS TABLE(ticket_id bigint, flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer, customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select t.id, f.id, a.id, a.name, a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets,
                           c.id, c.first_name, c.last_name, c.address, c.phone_number, c.credit_card_number from tickets t
                    join flights f on f.id = t.flight_id
                    join airline_companies a on a.id = f.airline_company_id
                    join customers c on c.id = t.customer_id
                    where t.id =_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_ticket(_id bigint) OWNER TO postgres;

--
-- Name: sp_get_ticket_history_by_original_id(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_ticket_history_by_original_id(_original_id bigint) RETURNS TABLE(id bigint, ticket_original_id bigint, flight_id bigint, customer_id bigint, customer_full_name text, customer_username text, ticket_status integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
            RETURN QUERY
            	select * from tickets_history t
                where t.original_ticket_id =_original_id;
            END;
$$;


ALTER FUNCTION public.sp_get_ticket_history_by_original_id(_original_id bigint) OWNER TO postgres;

--
-- Name: sp_get_tickets_by_airline_company(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_tickets_by_airline_company(_airline_company_id bigint) RETURNS TABLE(ticket_id bigint, flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer, customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select t.id, f.id, a.id, a.name, a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets,
                           c.id, c.first_name, c.last_name, c.address, c.phone_number, c.credit_card_number, c.user_id, u.username, u.password, u.email, u.user_role from tickets t
                    join flights f on f.id = t.flight_id
                    join airline_companies a on a.id = f.airline_company_id
                    join customers c on c.id = t.customer_id
					join users u on u.id = c.user_id
                    where a.id =_airline_company_id;
            END;
$$;


ALTER FUNCTION public.sp_get_tickets_by_airline_company(_airline_company_id bigint) OWNER TO postgres;

--
-- Name: sp_get_tickets_by_customer(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_tickets_by_customer(_customer_id bigint) RETURNS TABLE(ticket_id bigint, flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer, customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                      select t.id, f.id, a.id, a.name, a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets,
                           c.id, c.first_name, c.last_name, c.address, c.phone_number, c.credit_card_number, c.user_id, u.username, u.password, u.email, u.user_role  from tickets t
                    join flights f on f.id = t.flight_id
                    join airline_companies a on a.id = f.airline_company_id
                    join customers c on c.id = t.customer_id
					join users u on u.id = c.user_id
					where t.customer_id=_customer_id;
                END;
$$;


ALTER FUNCTION public.sp_get_tickets_by_customer(_customer_id bigint) OWNER TO postgres;

--
-- Name: sp_get_tickets_by_flight(bigint); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_tickets_by_flight(_flight_id bigint) RETURNS TABLE(ticket_id bigint, flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer, customer_id bigint, first_name text, last_name text, address text, phone_number text, credit_card_number text, user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                      select t.id, f.id, a.id, a.name, a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets,
                           c.id, c.first_name, c.last_name, c.address, c.phone_number, c.credit_card_number, c.user_id, u.username, u.password, u.email, u.user_role  from tickets t
                    join flights f on f.id = t.flight_id
                    join airline_companies a on a.id = f.airline_company_id
                    join customers c on c.id = t.customer_id
					join users u on u.id = c.user_id
					where t.flight_id=_flight_id;
                END;
$$;


ALTER FUNCTION public.sp_get_tickets_by_flight(_flight_id bigint) OWNER TO postgres;

--
-- Name: sp_get_user(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_user(_id integer) RETURNS TABLE(id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                RETURN QUERY
                    select * from users
                    where users.id =_id;
            END;
    $$;


ALTER FUNCTION public.sp_get_user(_id integer) OWNER TO postgres;

--
-- Name: sp_get_user_by_username(text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_user_by_username(_username text) RETURNS TABLE(user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select * from users u
                    where u.username =_username;
            END;
$$;


ALTER FUNCTION public.sp_get_user_by_username(_username text) OWNER TO postgres;

--
-- Name: sp_get_user_by_username_and_password(text, text); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_get_user_by_username_and_password(_username text, _password text) RETURNS TABLE(user_id bigint, username text, password text, email text, user_role_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select * from users u
                    where u.username =_username and u.password=_password;
            END;
$$;


ALTER FUNCTION public.sp_get_user_by_username_and_password(_username text, _password text) OWNER TO postgres;

--
-- Name: sp_remove_administrator(integer); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_remove_administrator(_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                DELETE FROM administrators WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_remove_administrator(_id integer) OWNER TO postgres;

--
-- Name: sp_remove_airline_company(bigint); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_remove_airline_company(_id bigint)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                DELETE FROM airline_companies WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_remove_airline_company(_id bigint) OWNER TO postgres;

--
-- Name: sp_remove_country(integer); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_remove_country(_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                DELETE FROM countries WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_remove_country(_id integer) OWNER TO postgres;

--
-- Name: sp_remove_customer(bigint); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_remove_customer(_id bigint)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                DELETE FROM customers WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_remove_customer(_id bigint) OWNER TO postgres;

--
-- Name: sp_remove_flight(bigint); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_remove_flight(_id bigint)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                DELETE FROM flights WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_remove_flight(_id bigint) OWNER TO postgres;

--
-- Name: sp_remove_ticket(bigint); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_remove_ticket(_id bigint)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                DELETE FROM tickets WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_remove_ticket(_id bigint) OWNER TO postgres;

--
-- Name: sp_remove_user(bigint); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_remove_user(_id bigint)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                DELETE FROM users WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_remove_user(_id bigint) OWNER TO postgres;

--
-- Name: sp_search_flights(integer, integer, date, date); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.sp_search_flights(_origin_country_id integer DEFAULT 0, _destination_country_id integer DEFAULT 0, _departure_date date DEFAULT NULL::date, _landing_date date DEFAULT NULL::date) RETURNS TABLE(flight_id bigint, airline_company_id bigint, airline_company_name text, airline_company_country_id integer, origin_country_id integer, destination_country_id integer, departure_time timestamp without time zone, landing_time timestamp without time zone, remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                RETURN QUERY
                    select f.id, a.id,a.name,a.country_id, f.origin_country_id, f.destination_country_id, f.departure_time, f.landing_time, f.remaining_tickets  from flights f
                    join airline_companies a on a.id = f.airline_company_id
					where (_origin_country_id = 0 or f.origin_country_id = _origin_country_id) AND
						  (_destination_country_id = 0 or f.destination_country_id = _destination_country_id) AND
						  (_departure_date IS NULL or f.departure_time:: date = _departure_date) AND
						  (_landing_date IS NULL or f.landing_time:: date = _landing_date);
                END;
$$;


ALTER FUNCTION public.sp_search_flights(_origin_country_id integer, _destination_country_id integer, _departure_date date, _landing_date date) OWNER TO postgres;

--
-- Name: sp_update_administrator(integer, text, text, integer); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_update_administrator(_id integer, _first_name text, _last_name text, _level integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                UPDATE administrators
                SET first_name=_first_name, last_name=_last_name, level=_level
                WHERE id=_id;
            END;
$$;


ALTER PROCEDURE public.sp_update_administrator(_id integer, _first_name text, _last_name text, _level integer) OWNER TO postgres;

--
-- Name: sp_update_airline_company(bigint, text, integer); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_update_airline_company(_id bigint, _name text, _country_id integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                UPDATE airline_companies
                SET name=_name, country_id=_country_id
                WHERE id=_id;
            END;
$$;


ALTER PROCEDURE public.sp_update_airline_company(_id bigint, _name text, _country_id integer) OWNER TO postgres;

--
-- Name: sp_update_country(integer, text); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_update_country(_id integer, _name text)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                UPDATE countries
                SET name=_name
                WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_update_country(_id integer, _name text) OWNER TO postgres;

--
-- Name: sp_update_customer(bigint, text, text, text, text, text); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_update_customer(_id bigint, _first_name text, _last_name text, _address text, _phone_number text, _credit_card_number text)
    LANGUAGE plpgsql
    AS $$
BEGIN
                UPDATE customers
                SET first_name=_first_name, last_name=_last_name, address=_address, phone_number=_phone_number,credit_card_number=_credit_card_number
                WHERE id=_id;
            END;
$$;


ALTER PROCEDURE public.sp_update_customer(_id bigint, _first_name text, _last_name text, _address text, _phone_number text, _credit_card_number text) OWNER TO postgres;

--
-- Name: sp_update_flight(bigint, integer, integer, timestamp without time zone, timestamp without time zone, integer); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_update_flight(_id bigint, _origin_country_id integer, _destination_country_id integer, _departure_time timestamp without time zone, _landing_time timestamp without time zone, _remaining_tickets integer)
    LANGUAGE plpgsql
    AS $$
BEGIN
                UPDATE flights
                SET origin_country_id=_origin_country_id, destination_country_id=_destination_country_id, departure_time=_departure_time,landing_time=_landing_time, remaining_tickets=_remaining_tickets
                WHERE id=_id;
            END;
$$;


ALTER PROCEDURE public.sp_update_flight(_id bigint, _origin_country_id integer, _destination_country_id integer, _departure_time timestamp without time zone, _landing_time timestamp without time zone, _remaining_tickets integer) OWNER TO postgres;

--
-- Name: sp_update_ticket(bigint, bigint, bigint); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_update_ticket(_id bigint, _flight_id bigint, _customer_id bigint)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                UPDATE tickets
                SET flight_id=_flight_id, customer_id=_customer_id
                WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_update_ticket(_id bigint, _flight_id bigint, _customer_id bigint) OWNER TO postgres;

--
-- Name: sp_update_user(bigint, text, text, text, integer); Type: PROCEDURE; Schema: public; Owner: postgres
--

CREATE PROCEDURE public.sp_update_user(_id bigint, _username text, _password text, _email text, _user_role_id integer)
    LANGUAGE plpgsql
    AS $$
            BEGIN
                UPDATE users
                SET username=_username, password=_password, email=_email, user_role=_user_role_id
                WHERE id=_id;
            END;
    $$;


ALTER PROCEDURE public.sp_update_user(_id bigint, _username text, _password text, _email text, _user_role_id integer) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: administrators; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.administrators (
    id integer NOT NULL,
    first_name text NOT NULL,
    last_name text NOT NULL,
    level integer DEFAULT 0 NOT NULL,
    user_id bigint NOT NULL
);


ALTER TABLE public.administrators OWNER TO postgres;

--
-- Name: administrators_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.administrators_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.administrators_id_seq OWNER TO postgres;

--
-- Name: administrators_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.administrators_id_seq OWNED BY public.administrators.id;


--
-- Name: airline_companies; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.airline_companies (
    id bigint NOT NULL,
    name text NOT NULL,
    country_id integer NOT NULL,
    user_id bigint NOT NULL
);


ALTER TABLE public.airline_companies OWNER TO postgres;

--
-- Name: airline_companies_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.airline_companies_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.airline_companies_id_seq OWNER TO postgres;

--
-- Name: airline_companies_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.airline_companies_id_seq OWNED BY public.airline_companies.id;


--
-- Name: customers; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.customers (
    id bigint NOT NULL,
    first_name text NOT NULL,
    last_name text NOT NULL,
    address text,
    phone_number text NOT NULL,
    credit_card_number text,
    user_id bigint NOT NULL
);


ALTER TABLE public.customers OWNER TO postgres;

--
-- Name: costumers_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.costumers_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.costumers_id_seq OWNER TO postgres;

--
-- Name: costumers_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.costumers_id_seq OWNED BY public.customers.id;


--
-- Name: countries; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.countries (
    id integer NOT NULL,
    name text NOT NULL
);


ALTER TABLE public.countries OWNER TO postgres;

--
-- Name: countries_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.countries_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.countries_id_seq OWNER TO postgres;

--
-- Name: countries_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.countries_id_seq OWNED BY public.countries.id;


--
-- Name: flights_history; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.flights_history (
    id bigint NOT NULL,
    flight_original_id bigint NOT NULL,
    airline_company_id bigint NOT NULL,
    airline_company_name text NOT NULL,
    origin_country_id integer NOT NULL,
    destination_country_id integer NOT NULL,
    departure_time timestamp without time zone NOT NULL,
    landing_time timestamp without time zone NOT NULL,
    remaining_tickets integer NOT NULL,
    flight_status integer NOT NULL
);


ALTER TABLE public.flights_history OWNER TO postgres;

--
-- Name: flight_history_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

ALTER TABLE public.flights_history ALTER COLUMN id ADD GENERATED ALWAYS AS IDENTITY (
    SEQUENCE NAME public.flight_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: flights; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.flights (
    id bigint NOT NULL,
    airline_company_id bigint NOT NULL,
    origin_country_id integer NOT NULL,
    destination_country_id integer NOT NULL,
    departure_time timestamp without time zone NOT NULL,
    landing_time timestamp without time zone NOT NULL,
    remaining_tickets integer DEFAULT 0 NOT NULL
);


ALTER TABLE public.flights OWNER TO postgres;

--
-- Name: flights_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.flights_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.flights_id_seq OWNER TO postgres;

--
-- Name: flights_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.flights_id_seq OWNED BY public.flights.id;


--
-- Name: tickets; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tickets (
    id bigint NOT NULL,
    flight_id bigint NOT NULL,
    customer_id bigint NOT NULL
);


ALTER TABLE public.tickets OWNER TO postgres;

--
-- Name: tickets_history; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.tickets_history (
    id bigint NOT NULL,
    original_ticket_id bigint NOT NULL,
    flight_id bigint NOT NULL,
    customer_id bigint NOT NULL,
    customer_full_name text,
    customer_username text,
    ticket_status integer
);


ALTER TABLE public.tickets_history OWNER TO postgres;

--
-- Name: tickets_history_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tickets_history_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tickets_history_id_seq OWNER TO postgres;

--
-- Name: tickets_history_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tickets_history_id_seq OWNED BY public.tickets_history.id;


--
-- Name: tickets_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.tickets_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.tickets_id_seq OWNER TO postgres;

--
-- Name: tickets_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.tickets_id_seq OWNED BY public.tickets.id;


--
-- Name: user_roles; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.user_roles (
    id integer NOT NULL,
    role_name text NOT NULL
);


ALTER TABLE public.user_roles OWNER TO postgres;

--
-- Name: user_roles_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.user_roles_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.user_roles_id_seq OWNER TO postgres;

--
-- Name: user_roles_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.user_roles_id_seq OWNED BY public.user_roles.id;


--
-- Name: users; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.users (
    id bigint NOT NULL,
    username text NOT NULL,
    password text NOT NULL,
    email text NOT NULL,
    user_role integer NOT NULL
);


ALTER TABLE public.users OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.users_id_seq
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.users_id_seq OWNER TO postgres;

--
-- Name: users_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.users_id_seq OWNED BY public.users.id;


--
-- Name: administrators id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.administrators ALTER COLUMN id SET DEFAULT nextval('public.administrators_id_seq'::regclass);


--
-- Name: airline_companies id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.airline_companies ALTER COLUMN id SET DEFAULT nextval('public.airline_companies_id_seq'::regclass);


--
-- Name: countries id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.countries ALTER COLUMN id SET DEFAULT nextval('public.countries_id_seq'::regclass);


--
-- Name: customers id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers ALTER COLUMN id SET DEFAULT nextval('public.costumers_id_seq'::regclass);


--
-- Name: flights id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.flights ALTER COLUMN id SET DEFAULT nextval('public.flights_id_seq'::regclass);


--
-- Name: tickets id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets ALTER COLUMN id SET DEFAULT nextval('public.tickets_id_seq'::regclass);


--
-- Name: tickets_history id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets_history ALTER COLUMN id SET DEFAULT nextval('public.tickets_history_id_seq'::regclass);


--
-- Name: user_roles id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_roles ALTER COLUMN id SET DEFAULT nextval('public.user_roles_id_seq'::regclass);


--
-- Name: users id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users ALTER COLUMN id SET DEFAULT nextval('public.users_id_seq'::regclass);


--
-- Data for Name: administrators; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.administrators (id, first_name, last_name, level, user_id) FROM stdin;
\.


--
-- Data for Name: airline_companies; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.airline_companies (id, name, country_id, user_id) FROM stdin;
\.


--
-- Data for Name: countries; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.countries (id, name) FROM stdin;
\.


--
-- Data for Name: customers; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.customers (id, first_name, last_name, address, phone_number, credit_card_number, user_id) FROM stdin;
\.


--
-- Data for Name: flights; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.flights (id, airline_company_id, origin_country_id, destination_country_id, departure_time, landing_time, remaining_tickets) FROM stdin;
\.


--
-- Data for Name: flights_history; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.flights_history (id, flight_original_id, airline_company_id, airline_company_name, origin_country_id, destination_country_id, departure_time, landing_time, remaining_tickets, flight_status) FROM stdin;
\.


--
-- Data for Name: tickets; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tickets (id, flight_id, customer_id) FROM stdin;
\.


--
-- Data for Name: tickets_history; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.tickets_history (id, original_ticket_id, flight_id, customer_id, customer_full_name, customer_username, ticket_status) FROM stdin;
\.


--
-- Data for Name: user_roles; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.user_roles (id, role_name) FROM stdin;
1	administrator
2	customer
3	airline company
\.


--
-- Data for Name: users; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public.users (id, username, password, email, user_role) FROM stdin;
\.


--
-- Name: administrators_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.administrators_id_seq', 1, false);


--
-- Name: airline_companies_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.airline_companies_id_seq', 1, false);


--
-- Name: costumers_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.costumers_id_seq', 1, false);


--
-- Name: countries_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.countries_id_seq', 1, false);


--
-- Name: flight_history_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.flight_history_id_seq', 1, false);


--
-- Name: flights_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.flights_id_seq', 1, false);


--
-- Name: tickets_history_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.tickets_history_id_seq', 1, false);


--
-- Name: tickets_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.tickets_id_seq', 1, false);


--
-- Name: user_roles_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.user_roles_id_seq', 3, true);


--
-- Name: users_id_seq; Type: SEQUENCE SET; Schema: public; Owner: postgres
--

SELECT pg_catalog.setval('public.users_id_seq', 1, false);


--
-- Name: administrators administrators_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.administrators
    ADD CONSTRAINT administrators_pk PRIMARY KEY (id);


--
-- Name: airline_companies airline_companies_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.airline_companies
    ADD CONSTRAINT airline_companies_pk PRIMARY KEY (id);


--
-- Name: customers costumers_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT costumers_pk PRIMARY KEY (id);


--
-- Name: countries countries_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.countries
    ADD CONSTRAINT countries_pk PRIMARY KEY (id);


--
-- Name: flights_history flight_history_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.flights_history
    ADD CONSTRAINT flight_history_pkey PRIMARY KEY (id);


--
-- Name: flights flights_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.flights
    ADD CONSTRAINT flights_pk PRIMARY KEY (id);


--
-- Name: tickets_history tickets_history_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets_history
    ADD CONSTRAINT tickets_history_pk PRIMARY KEY (id);


--
-- Name: tickets tickets_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_pk PRIMARY KEY (id);


--
-- Name: user_roles user_roles_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.user_roles
    ADD CONSTRAINT user_roles_pk PRIMARY KEY (id);


--
-- Name: users users_pk; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_pk PRIMARY KEY (id);


--
-- Name: administrators_user_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX administrators_user_id_uindex ON public.administrators USING btree (user_id);


--
-- Name: airline_companies_name_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX airline_companies_name_uindex ON public.airline_companies USING btree (name);


--
-- Name: airline_companies_user_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX airline_companies_user_id_uindex ON public.airline_companies USING btree (user_id);


--
-- Name: costumers_phone_number_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX costumers_phone_number_uindex ON public.customers USING btree (phone_number);


--
-- Name: costumers_user_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX costumers_user_id_uindex ON public.customers USING btree (user_id);


--
-- Name: countries_name_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX countries_name_uindex ON public.countries USING btree (name);


--
-- Name: flight_original_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX flight_original_id_uindex ON public.flights_history USING btree (flight_original_id);


--
-- Name: tickets_flight_id_customer_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX tickets_flight_id_customer_id_uindex ON public.tickets USING btree (flight_id, customer_id);


--
-- Name: tickets_history_flight_id_customer_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX tickets_history_flight_id_customer_id_uindex ON public.tickets_history USING btree (flight_id, customer_id);


--
-- Name: tickets_history_original_ticket_id_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX tickets_history_original_ticket_id_uindex ON public.tickets_history USING btree (original_ticket_id);


--
-- Name: user_roles_role_name_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX user_roles_role_name_uindex ON public.user_roles USING btree (role_name);


--
-- Name: users_email_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_email_uindex ON public.users USING btree (email);


--
-- Name: users_username_uindex; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX users_username_uindex ON public.users USING btree (username);


--
-- Name: administrators administrators_users_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.administrators
    ADD CONSTRAINT administrators_users_id_fk FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: airline_companies airline_companies_countries_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.airline_companies
    ADD CONSTRAINT airline_companies_countries_id_fk FOREIGN KEY (country_id) REFERENCES public.countries(id);


--
-- Name: airline_companies airline_companies_users_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.airline_companies
    ADD CONSTRAINT airline_companies_users_id_fk FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: customers costumers_users_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.customers
    ADD CONSTRAINT costumers_users_id_fk FOREIGN KEY (user_id) REFERENCES public.users(id);


--
-- Name: flights flights_airline_companies_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.flights
    ADD CONSTRAINT flights_airline_companies_id_fk FOREIGN KEY (airline_company_id) REFERENCES public.airline_companies(id);


--
-- Name: flights flights_countries_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.flights
    ADD CONSTRAINT flights_countries_id_fk FOREIGN KEY (origin_country_id) REFERENCES public.countries(id);


--
-- Name: flights flights_countries_id_fk_2; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.flights
    ADD CONSTRAINT flights_countries_id_fk_2 FOREIGN KEY (destination_country_id) REFERENCES public.countries(id);


--
-- Name: tickets tickets_customers_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_customers_id_fk FOREIGN KEY (customer_id) REFERENCES public.customers(id);


--
-- Name: tickets tickets_flights_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.tickets
    ADD CONSTRAINT tickets_flights_id_fk FOREIGN KEY (flight_id) REFERENCES public.flights(id);


--
-- Name: users users_user_roles_id_fk; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.users
    ADD CONSTRAINT users_user_roles_id_fk FOREIGN KEY (user_role) REFERENCES public.user_roles(id);


--
-- PostgreSQL database dump complete
--

